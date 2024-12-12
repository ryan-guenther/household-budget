using System.Transactions;

using HouseholdBudget.Domain.Interfaces;

namespace HouseholdBudget.Domain.Entities
{
    public enum AccountType
    {
        Chequing,
        Savings,
        CreditCard,
        Loan,
        Other
    }

    public class Account : IEntity, ITrackable
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;  // Account name (e.g., "My Chequing Account")
        public AccountType Type { get; set; }  // Type of account (Chequing, Savings, etc.)
        public decimal Balance { get; set; }  // Current balance of the account
        public List<Transaction> Transactions { get; set; } = new List<Transaction>();  // Transactions related to the account

        // Tracking properties from ITrackable
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.MinValue;
        public string? ModifiedBy { get; set; } = string.Empty;
        public DateTime? ModifiedAt { get; set; } = DateTime.MinValue;


        public Account() { }

        public Account(int id, string name, AccountType accountType, decimal balance)
        {
            Id = id;
            Name = name;
            Type = accountType;
            Balance = balance;
        }
    }
}
