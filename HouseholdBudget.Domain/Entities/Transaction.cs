using HouseholdBudget.Domain.Interfaces;

namespace HouseholdBudget.Domain.Entities
{
    public enum TransactionType
    {
        Expense,
        Credit
    }

    public class Transaction : BaseEntity, IEntity, ITrackable
    {
        public decimal Amount { get; set; }  // Amount for the transaction (positive for Credit, negative for Expense)
        public DateTime Date { get; set; }  // Date of the transaction
        public string Description { get; set; } = string.Empty;  // Description of the transaction
        public TransactionType Type { get; set; }  // Type of transaction (Expense or Credit)
        public int AccountId { get; set; }  // Foreign Key for the related Account
        public Account Account { get; set; } = new Account();  // Navigation property to Account

        // Tracking properties from ITrackable
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.MinValue;
        public string? ModifiedBy { get; set; } = string.Empty;
        public DateTime? ModifiedAt { get; set; } = DateTime.MinValue;

        public Transaction()
        {
            
        }

        public Transaction(int id, decimal amount, DateTime date, string description, TransactionType type, Account account)
        {
            Id = id;
            Amount = amount;
            Date = date;
            Description = description;
            Type = type;
            AccountId = account.Id;
            Account = account;
        }
    }
}