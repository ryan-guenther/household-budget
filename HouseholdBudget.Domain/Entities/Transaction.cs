using HouseholdBudget.Domain.Interfaces;

namespace HouseholdBudget.Domain.Entities
{
    public enum TransactionType
    {
        Expense,
        Credit
    }

    public class Transaction : IEntity, ITrackable
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }  // Amount for the transaction (positive for Credit, negative for Expense)
        public DateTime Date { get; set; }  // Date of the transaction
        public string Description { get; set; } = string.Empty;  // Description of the transaction
        public TransactionType Type { get; set; }  // Type of transaction (Expense or Credit)
        public int AccountId { get; set; }  // Foreign Key for the related Account
        public Account Account { get; set; }  // Navigation property to Account

        // Tracking properties from ITrackable
        public string CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedAt { get; set; }
    }
}