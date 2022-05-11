using homebudget_server.Data;
using homebudget_server.Models.ViewModels;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace homebudget_server.Controllers
{
    [ApiController]
    [EnableCors("AllowAll")]
    [Route("[controller]")]
    public class TransactionController : ControllerBase
    {
        private readonly ILogger<TransactionController> _logger;
        private readonly HomebudgetContext _context;

        public TransactionController(ILogger<TransactionController> logger, HomebudgetContext context)
        {
            _logger = logger;
            _context = context;
        }

        [HttpPost(Name = "CreateTransaction")]
        public ViewTransaction Create(ViewCreateTransaction model)
        {
            if (ModelState.IsValid)
            {
                using (var transaction = _context.Database.BeginTransaction())
                {
                    var result = new Models.Transaction
                    {
                        DateTime = model.DateTime,
                        SourceAccountId = model.SourceId,
                        Description = model.Description,
                        DestinationAccountId = model.DestinationId,
                        Identifier = model.Identifier,
                        TransactionLines = model.Lines.Select((line, i) => new Models.TransactionLine
                        {
                            Amount = line.Amount,
                            Description = line.Description,
                            Order = i + 1,
                        }).ToList(),
                    };
                    _context.Transactions.Add(result);
                    transaction.Commit();
                    _context.SaveChanges();
                    return new ViewTransaction {
                        Id = result.Id,
                        Identifier = result.Identifier,
                        DateTime = result.DateTime,
                        Description = result.Description,
                        Source = result.SourceAccount != null
                            ? new ViewAccount
                            {
                                Id = result.SourceAccount.Id,
                                Name = result.SourceAccount.Name,
                                Description = result.SourceAccount.Description,
                            } : null,
                        Destination = result.DestinationAccount != null
                            ? new ViewAccount
                            {
                                Id = result.DestinationAccount.Id,
                                Name = result.DestinationAccount.Name,
                                Description = result.DestinationAccount.Description,
                            } : null,
                        Lines = result.TransactionLines
                            .OrderBy(line => line.Order)
                            .Select(line => new ViewTransactionLine
                            {
                                Amount = line.Amount,
                            }).ToList(),
                    };
                }
            }
            throw new Exception();
        }

        [HttpPost()]
        [Route("/[controller]/[action]")]
        public ViewSearchResponse<ViewTransaction> Search(ViewSearch query)
        {
            var result = _context.Transactions
                .ApplySearch(query)
                .Skip(query.From)
                .Take(query.To - query.From)
                .Select(transaction => new ViewTransaction
                {
                    Id = transaction.Id,
                    DateTime = transaction.DateTime,
                    Description = transaction.Description,
                    Source = transaction.SourceAccount != null
                        ? new ViewAccount
                        {
                            Id = transaction.SourceAccount.Id,
                            Description = transaction.SourceAccount.Description,
                            Name = transaction.SourceAccount.Name
                        } : null,
                    Destination = transaction.DestinationAccount != null
                        ? new ViewAccount
                        {
                            Id = transaction.DestinationAccount.Id,
                            Description = transaction.DestinationAccount.Description,
                            Name = transaction.DestinationAccount.Name
                        } : null,
                    Identifier = transaction.Identifier,
                    Lines = transaction.TransactionLines
                    .OrderBy(line => line.Order)
                    .Select(line => new ViewTransactionLine
                    {
                        Amount = line.Amount,
                    }).ToList()
                })
                .ToList();

            return new ViewSearchResponse<ViewTransaction>
            {
                Data = result,
                TotalItems = _context.Transactions.ApplySearch(query).Count(),
            };
        }

        [HttpPost()]
        [Route("/[controller]/[action]")]
        public ViewTransactionList List(ViewTransactionListRequest request)
        {
            var query = _context.Transactions
                .Select(transaction => new ViewTransaction
                {
                    Id = transaction.Id,
                    DateTime = transaction.DateTime,
                    Description = transaction.Description,
                    Source = transaction.SourceAccount != null
                        ? new ViewAccount
                        {
                            Id = transaction.SourceAccount.Id,
                            Description = transaction.SourceAccount.Description,
                            Name = transaction.SourceAccount.Name
                        } : null,
                    Destination = transaction.DestinationAccount != null
                        ? new ViewAccount
                        {
                            Id = transaction.DestinationAccount.Id,
                            Description = transaction.DestinationAccount.Description,
                            Name = transaction.DestinationAccount.Name
                        } : null,
                    Identifier = transaction.Identifier,
                    Lines = transaction.TransactionLines
                    .OrderBy(line => line.Order)
                    .Select(line => new ViewTransactionLine
                    {
                        Amount = line.Amount,
                    }).ToList()
                });

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
                .ToList();
            var firstTransaction = result
                .OrderBy(transaction => transaction.DateTime)
                .ThenBy(transaction => transaction.Id).FirstOrDefault();
            var total = firstTransaction == null ? 0 : _context.Transactions
                .Where(transaction => transaction.DateTime <= firstTransaction.DateTime && transaction.Id < firstTransaction.Id)
                .SelectMany(transaction => transaction.TransactionLines.Select(line => line.Amount))
                .Sum();

            return new ViewTransactionList
            {
                Data = result,
                TotalItems = _context.Transactions.Count(),
                Total = total,
            };
        }

        [HttpPost()]
        [Route("/[controller]/[action]")]
        public List<string> FindDuplicates(List<string> identifiers)
        {
            return _context.Transactions
                .Where(transaction => transaction.Identifier != null && identifiers.Contains(transaction.Identifier))
                .Select(transaction => transaction.Identifier!)
                .ToList();
        }

        [HttpPost()]
        [Route("/[controller]/[action]")]
        public ViewTransactionCreateManyResponse CreateMany(List<ViewCreateTransaction> transactions)
        {
            var failed = new List<ViewCreateTransaction>();
            var duplicate = new List<ViewCreateTransaction>();
            var success = new List<ViewCreateTransaction>();

            foreach (var transaction in transactions)
            {
                if (! string.IsNullOrWhiteSpace(transaction.Identifier) && _context.Transactions.Any(dbTransaction => dbTransaction.Identifier == transaction.Identifier))
                {
                    duplicate.Add(transaction);
                }
                else
                {
                    try
                    {
                        var result = new Models.Transaction
                        {
                            DateTime = transaction.DateTime,
                            SourceAccountId = transaction.SourceId,
                            Description = transaction.Description,
                            DestinationAccountId = transaction.DestinationId,
                            Identifier = transaction.Identifier,
                            TransactionLines = transaction.Lines.Select((line, i) => new Models.TransactionLine
                            {
                                Amount = line.Amount,
                                Description = line.Description,
                                Order = i + 1,
                            }).ToList(),
                        };
                        _context.Transactions.Add(result);
                        _context.SaveChanges();
                        success.Add(transaction);
                    }
                    catch (Exception)
                    {
                        failed.Add(transaction);
                    }
                }
            }

            return new ViewTransactionCreateManyResponse
            {
                Succeeded = success,
                Duplicate = duplicate,
                Failed = failed,
            };
        }
    }
}