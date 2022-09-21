﻿using homebudget_server.Data;
using homebudget_server.Models;
using homebudget_server.Models.ViewModels;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace homebudget_server.Controllers
{
    [ApiController]
    [EnableCors("AllowAll")]
    [Route("[controller]")]
    public class AccountController : Controller
    {
        private readonly ILogger<TransactionController> _logger;
        private readonly HomebudgetContext _context;

        public AccountController(ILogger<TransactionController> logger, HomebudgetContext context)
        {
            _logger = logger;
            _context = context;
        }

        [HttpPost()]
        public ViewAccount Create(ViewCreateAccount model)
        {
            if (ModelState.IsValid)
            {
                var result = new Account
                {
                    Name = model.Name,
                    Description = model.Description,
                    AccountNumber = model.AccountNumber,
                    IncludeInNetWorth = model.IncludeInNetWorth,
                    Favorite = model.Favorite,
                };
                _context.Accounts.Add(result);
                _context.SaveChanges();
                return new ViewAccount
                {
                    Id = result.Id,
                    AccountNumber = result.AccountNumber,
                    Description = result.Description,
                    Name = result.Name,
                    Favorite = model.Favorite,
                };
            }
            throw new Exception();
        }

        struct AccountMovementGroup
        {
            public DateTime TimePoint { get; set; }
            public string Type { get; set; }
            public long Total { get; set; }
        }

        [HttpPost()]
        [Route("/[controller]/{id}/Movements")]
        public ViewGetMovementResponse GetMovement(int id, ViewGetMovementRequest request)
        {
            var initialBalance = request.From == null ? 0 :
                _context.Transactions
                    .Where(transaction => transaction.DateTime < request.From)
                    .Where(transaction => transaction.SourceAccountId == id || transaction.DestinationAccountId == id)
                    .Select(transaction => transaction.DestinationAccountId == id ? transaction.Total : -transaction.Total)
                    .Sum();

            var statsRequest = _context.Transactions
                .Where(transaction => request.From == null || transaction.DateTime >= request.From)
                .Where(transaction => request.To == null || transaction.DateTime <= request.To)
                .Where(transaction => transaction.SourceAccountId == id || transaction.DestinationAccountId == id);

            List <AccountMovementGroup> result;
            switch (request.Resolution)
            {
                case AccountMovementResolution.Daily:
                    result = statsRequest.GroupBy(transaction => new
                    {
                        type = transaction.DestinationAccountId == id ? "revenue" : "expense",
                        timepoint = transaction.DateTime.Year.ToString() + "-" + transaction.DateTime.Month.ToString() + "-" + transaction.DateTime.Day.ToString()
                    }).Select(group => new AccountMovementGroup { 
                        Type = group.Key.type,
                        TimePoint = DateTime.ParseExact(group.Key.timepoint, "yyyy-M-d", CultureInfo.InvariantCulture),
                        Total = group.Select(transaction => transaction.Total).Sum(),
                    }).ToList();
                    break;
                case AccountMovementResolution.Weekly:
                    var offset = request.From ?? new DateTime(1900, 01, 01);
                    result = statsRequest.Select(transaction => new
                    {
                        transaction,
                        week = Math.Floor(EF.Functions.DateDiffDay(offset, transaction.DateTime) / 7.0)
                    }).GroupBy(obj => new
                    {
                        type = obj.transaction.DestinationAccountId == id ? "revenue" : "expense",
                        timepoint = obj.week,
                    }).Select(group => new AccountMovementGroup
                    {
                        Type = group.Key.type,
                        TimePoint = offset.AddDays(group.Key.timepoint * 7),
                        Total = group.Select(obj => obj.transaction.Total).Sum(),
                    }).ToList();
                    break;
                case AccountMovementResolution.Monthly:
                    result = statsRequest.GroupBy(transaction => new
                    {
                        type = transaction.DestinationAccountId == id ? "revenue" : "expense",
                        timepoint = transaction.DateTime.Year.ToString() + "-" + transaction.DateTime.Month.ToString()
                    }).Select(group => new AccountMovementGroup
                    {
                        Type = group.Key.type,
                        TimePoint = DateTime.ParseExact(group.Key.timepoint + "-01", "yyyy-M-dd", CultureInfo.InvariantCulture),
                        Total = group.Select(transaction => transaction.Total).Sum(),
                    }).ToList();
                    break;
                case AccountMovementResolution.Yearly:
                    result = statsRequest.GroupBy(transaction => new
                    {
                        type = transaction.DestinationAccountId == id ? "revenue" : "expense",
                        timepoint = transaction.DateTime.Year.ToString()
                    }).Select(group => new AccountMovementGroup
                    {
                        Type = group.Key.type,
                        TimePoint = DateTime.ParseExact(group.Key.timepoint + "-01-01", "yyyy-MM-dd", CultureInfo.InvariantCulture),
                        Total = group.Select(transaction => transaction.Total).Sum(),
                    }).ToList();
                    break;
                default:
                    throw new Exception($"Unknown resolution {request.Resolution}");
            }

            var revenue = result.Where(item => item.Type == "revenue").ToDictionary(item => item.TimePoint, item => item.Total);
            var expenses = result.Where(item => item.Type == "expense").ToDictionary(item => item.TimePoint, item => item.Total);
            foreach (var key in revenue.Keys)
            {
                if (!expenses.ContainsKey(key)) expenses[key] = 0;
            }
            foreach (var key in expenses.Keys)
            {
                if (!revenue.ContainsKey(key)) revenue[key] = 0;
            }

            return new ViewGetMovementResponse
            {
                InitialBalance = initialBalance,
                Items = expenses.Join(revenue,
                        result => result.Key,
                        result => result.Key,
                        (a, b) => new ViewAccountMovementItem
                        {
                            DateTime = a.Key,
                            Expenses = a.Value,
                            Revenue = b.Value,
                        })
                    .OrderBy(item => item.DateTime).ToList()
            };
        }

        [HttpPost()]
        [Route("/[controller]/{id}/[action]")]
        public List<object> CategorySummary(int id, ViewSearchGroup query)
        {
            return _context.Transactions
                .Where(transaction => transaction.SourceAccountId == id || transaction.DestinationAccountId == id)
                .ApplySearch(query)
                .GroupBy(transaction => transaction.Category != null ? transaction.Category.Name : "")
                .Select(group => (object)new
                {
                    category = group.Key,
                    revenue = group.Where(t => t.DestinationAccountId == id).Select(t => t.Total).Sum(),
                    expenses = group.Where(t => t.SourceAccountId == id).Select(t => t.Total).Sum()
                })
                .ToList();
        }

        [HttpGet()]
        [Route("/[controller]/{id}")]
        public ViewAccount? Get(int id)
        {
            var result = _context.Accounts
                .SelectView()
                .SingleOrDefault(account => account.Id == id);

            if (result == null) return null;

            var balance = _context.Transactions
                .Where(transaction => transaction.SourceAccountId == id || transaction.DestinationAccountId == id)
                .Select(transaction => transaction.DestinationAccountId == id ? transaction.Total : -transaction.Total)
                .Sum();

            result.Balance = balance;

            return result;
        }

        [HttpPut()]
        [Route("/[controller]/{id}")]
        public ViewAccount? Update(int id, ViewAccount model)
        {
            var result = _context.Accounts
                .SingleOrDefault(account => account.Id == id);

            if (result == null)
            {
                throw new Exception($"Account not found with id {id}");
            }

            result.AccountNumber = model.AccountNumber;
            result.Description = model.Description;
            result.Name = model.Name;
            result.Favorite = model.Favorite;
            result.IncludeInNetWorth = model.IncludeInNetWorth;

            _context.SaveChanges();

            return new ViewAccount
            {
                Id = result.Id,
                Name = result.Name,
                Description = result.Description,
                AccountNumber = result.AccountNumber,
                Favorite = result.Favorite,
                IncludeInNetWorth = result.IncludeInNetWorth,
            };
        }

        [HttpDelete()]
        [Route("/[controller]/{id}")]
        public void Delete(int id)
        {
            using (var transaction = _context.Database.BeginTransaction())
            {
                var dbObject = _context.Accounts
                    .Single(t => t.Id == id);

                // Remove all references to this account on transactions
                _context.Database.ExecuteSqlInterpolated($"UPDATE Transactions SET SourceAccountId = null WHERE SourceAccountId = {id}");
                _context.Database.ExecuteSqlInterpolated($"UPDATE Transactions SET DestinationAccountId = null WHERE DestinationAccountId = {id}");
                // Delete transactions that do not have any accounts
                _context.Database.ExecuteSqlInterpolated($"DELETE FROM Transactions WHERE SourceAccountId IS NULL AND DestinationAccountId IS NULL");
                _context.Accounts.Remove(dbObject);
                _context.SaveChanges();
                // Delete all categories that are unused by transactions
                _context.Categories.RemoveRange(_context.Categories.Where(category => !_context.Transactions.Any(transaction => transaction.CategoryId == category.Id)));
                _context.SaveChanges();

                transaction.Commit();

                return;
            }
            throw new Exception();
        }

        [HttpPost()]
        [Route("/[controller]/[action]")]
        public ViewSearchResponse<ViewAccount> Search(ViewSearch query)
        {
            var result = _context.Accounts
                .ApplySearch(query, true)
                .Skip(query.From)
                .Take(query.To - query.From)
                .SelectView()
                .ToList();

            return new ViewSearchResponse<ViewAccount>
            {
                Data = result,
                TotalItems = _context.Accounts.ApplySearch(query, false).Count()
            };
        }

        [HttpPost()]
        [Route("/[controller]/{id}/[action]")]
        public ViewTransactionList Transactions(int id, ViewTransactionListRequest request)
        {
            var query = _context.Transactions
                .Where(transaction => transaction.SourceAccountId == id || transaction.DestinationAccountId == id);

            if (request.Query != null)
            {
                query = query.ApplySearch(request.Query);
            }

            if (request.Descending)
            {
                query = query.OrderByDescending(transaction => transaction.DateTime)
                    .ThenByDescending(transaction => transaction.Id);
            }
            else
            {
                query = query.OrderBy(transaction => transaction.DateTime)
                    .ThenBy(transaction => transaction.Id);
            }

            var result = query
                .Skip(request.From)
                .Take(request.To - request.From)
                .SelectView()
                .ToList();
            var firstTransaction = result
                .OrderBy(transaction => transaction.DateTime)
                .ThenBy(transaction => transaction.Id).FirstOrDefault();
            var total = firstTransaction == null ? 0 : _context.Transactions
                .Where(transaction => transaction.SourceAccountId == id || transaction.DestinationAccountId == id)
                .Where(transaction => transaction.DateTime < firstTransaction.DateTime || (transaction.DateTime == firstTransaction.DateTime && transaction.Id < firstTransaction.Id))
                .Select(transaction => transaction.Total * (transaction.DestinationAccountId == id ? 1 : -1))
                .Sum();

            return new ViewTransactionList
            {
                Data = result,
                TotalItems = query.Count(),
                Total = total,
            };
        }

        /// <summary>
        /// Same as the method to search transactions, but only returns the count.
        /// Used to figure out which page is the last without requesting any page
        /// </summary>
        [HttpPost()]
        [Route("/[controller]/{id}/[action]")]
        public int CountTransactions(int id, ViewSearchGroup? requestQuery)
        {
            var query = _context.Transactions
                .Where(transaction => transaction.SourceAccountId == id || transaction.DestinationAccountId == id);

            if (requestQuery != null)
            {
                query = query.ApplySearch(requestQuery);
            }

            return query.Count();
        }
    }
}
