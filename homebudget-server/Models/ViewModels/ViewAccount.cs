﻿namespace homebudget_server.Models.ViewModels
{
    public class ViewAccount
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string? AccountNumber { get; set; }
        public bool Favorite { get; set; }
        public long Balance { get; set; }
        public string BalanceString { get => Balance.ToString(); set => Balance = long.Parse(value); }
    }

    public class ViewCreateAccount
    {
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;

        public string? AccountNumber { get; set; }
    }

    public class GetAccountResponse
    {
        public ViewAccount Account { get; set; } = null!;
    }
}
