﻿using homebudget_server.Data;
using homebudget_server.Models.ViewModels;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

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
        public ViewAccount Create(CreateViewAccount model)
        {
            if (ModelState.IsValid)
            {
                var result = new Models.Account
                {
                    Name = model.Name,
                    Description = model.Description,
                    AccountNumber = model.AccountNumber
                };
                _context.Accounts.Add(result);
                _context.SaveChanges();
                return new ViewAccount
                {
                    Id = result.Id,
                    AccountNumber = result.AccountNumber,
                    Description = result.Description,
                    Name = result.Name
                };
            }
            throw new Exception();
        }

        [HttpGet()]
        [Route("/[controller]/{id}")]
        public ViewAccount? Get(int id)
        {
            var result = _context.Accounts
                .Select(account => new ViewAccount
                {
                    Id = account.Id,
                    Name = account.Name,
                    AccountNumber = account.AccountNumber,
                    Description = account.Description
                })
                .SingleOrDefault(account => account.Id == id);

            return result;
        }

        [HttpPost()]
        [Route("/[controller]/[action]")]
        public ViewSearchResponse<ViewAccount> Search(ViewSearch query)
        {
            var result = _context.Accounts
                .ApplySearch(query)
                .Skip(query.From)
                .Take(query.To - query.From)
                .ToList()
                .Select(account => new ViewAccount
                {
                    Id = account.Id,
                    Name = account.Name,
                    AccountNumber = account.AccountNumber,
                    Description = account.Description
                })
                .ToList();

            return new ViewSearchResponse<ViewAccount>
            {
                Data = result,
                TotalItems = _context.Accounts.ApplySearch(query).Count()
            };
        }
    }
}
