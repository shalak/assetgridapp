﻿using assetgrid_backend.Data;
using assetgrid_backend.Models;
using assetgrid_backend.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace assetgrid_backend.Services
{
    public interface IUserService
    {
        UserAuthenticatedResponse? Authenticate(string email, string password);
        User CreateUser(string email, string password);
        UserPreferences GetPreferences(User user);
        UserPreferences GetPreferences(int userId);
        User GetById(int id);
        User? GetCurrent(HttpContext context);
        bool ValidatePassword(User user, string password);
        void SetPassword(User user, string password);
    }

    public class UserService : IUserService
    {
        private readonly string _jwtSecret;
        private readonly AssetgridDbContext _context;

        public User? MockUser { get; set; }
        public UserService(JwtSecret secret, AssetgridDbContext context)
        {
            _jwtSecret = secret.Value;
            _context = context;
        }

        public UserAuthenticatedResponse? Authenticate(string email, string password)
        {
            // Force entity framework to load favorite accounts for the user
            var user = _context.Users
                .Include(user => user.Preferences)
                .Where(user => user.NormalizedEmail == email.ToLower())
                .Select(user => new
                {
                    user,
                    favoriteAccounts = _context.UserAccounts.Include(account => account.Account).Where(account => account.Favorite && account.UserId == user.Id).ToList()
                }).SingleOrDefault();

            // return null if user not found
            if (user == null) return null;

            // Verify password
            if (! ValidatePassword(user.user, password))
            {
                return null;
            }

            // authentication successful so generate jwt token
            var token = generateJwtToken(user.user);

            return new UserAuthenticatedResponse(
                user.user.Id,
                user.user.Email,
                new ViewPreferences(GetPreferences(user.user)),
                user.favoriteAccounts.Select(account => new ViewAccount(
                    account.Id,
                    account.Account.Name,
                    account.Account.Description,
                    account.Account.AccountNumber,
                    account.Favorite,
                    account.IncludeInNetWorth,
                    ViewAccount.PermissionsFromDbPermissions(account.Permissions),
                    0)).ToList(),
                token);
        }

        public bool ValidatePassword(User user, string password)
        {
            var hasher = new PasswordHasher<string>();
            var result = hasher.VerifyHashedPassword(user.Email.ToLower(), user.HashedPassword, password);
            switch (result)
            {
                case PasswordVerificationResult.SuccessRehashNeeded:
                    user.HashedPassword = hasher.HashPassword(user.Email.ToLower(), password);
                    _context.SaveChanges();
                    return true;
                case PasswordVerificationResult.Success:
                    return true;
                default:
                    return false ;
            }
        }

        public void SetPassword(User user, string value)
        {
            var hasher = new PasswordHasher<string>();
            user.HashedPassword = hasher.HashPassword(user.Email.ToLower(), value);
        }

        public User CreateUser(string email, string password)
        {
            var user = new User
            {
                Email = email,
                NormalizedEmail = email.ToLower(),
            };
            SetPassword(user, password);
            _context.Users.Add(user);
            _context.SaveChanges();

            return user;
        }

        public UserPreferences GetPreferences(User user)
        {
            return preferencesOrDefault(user.Preferences);
        }

        public UserPreferences GetPreferences(int userId)
        {
            var preferences = _context.UserPreferences.SingleOrDefault(preferences => preferences.UserId == userId);
            return preferencesOrDefault(preferences);
        }

        private UserPreferences preferencesOrDefault(UserPreferences? preferences)
        {
            return preferences ?? new UserPreferences
            {
                ThousandsSeparator = ",",
                DecimalSeparator = ".",
                DecimalDigits = 2,
                DateFormat = null,
                DateTimeFormat = null,
            };
        }

        public User GetById(int id)
        {
            return _context.Users.Single(user => user.Id == id);
        }

        public User? GetCurrent(HttpContext context)
        {
            if (MockUser != null)
            {
                return MockUser;
            }
            else
            {
                return (User?)context?.Items["User"];
            }
        }

        private string generateJwtToken(User user)
        {
            // generate token that is valid for 7 days
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSecret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { new Claim("id", user.Id.ToString()) }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}