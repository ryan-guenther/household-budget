namespace HouseholdBudget.DTO.Transaction;
public abstract class TransactionResponseDTO : BaseDTO
{
    public decimal Amount { get; set; }  // Transaction amount
    public DateTime Date { get; set; }  // Date of the transaction
    public string Type { get; set; } = string.Empty;  // Transaction type (e.g., "Expense" or "Credit")
}

public sealed class TransactionDetailResponseDTO : TransactionResponseDTO
{
    public string Description { get; set; } = string.Empty;  // Description of the transaction
    public int AccountId { get; set; }  // Foreign key to the Account
}

public sealed class TransactionListResponseDTO : TransactionResponseDTO
{

}