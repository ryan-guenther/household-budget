﻿namespace HouseholdBudget.DTO.Transaction;
public abstract class TransactionRequestDTO : BaseDTO
{
    public decimal Amount { get; set; }  // Transaction amount
    public DateTime Date { get; set; }  // Date of the transaction
    public string Description { get; set; } = string.Empty;  // Description of the transaction
    public string Type { get; set; } = string.Empty;  // Transaction type (e.g., "Expense" or "Credit")
}

public sealed class TransactionCreateRequestDTO : TransactionRequestDTO
{
    public int AccountId { get; set; }  // Foreign key to the Account
}

public sealed class TransactionUpdateRequestDTO : TransactionRequestDTO
{

}