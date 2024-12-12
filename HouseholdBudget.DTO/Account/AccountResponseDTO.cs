namespace HouseholdBudget.DTO.Account;
public abstract class AccountResponseDTO : BaseDTO
{
    public string Type { get; set; } = string.Empty;  // Account type (e.g., "Chequing", "Savings", etc.)
    public decimal Balance { get; set; }  // Current balance
}

public sealed class AccountDetailResponseDTO : AccountResponseDTO
{
    public string Name { get; set; } = string.Empty;  // Account name (e.g., "Chequing Account")
}

public sealed class AccountListResponseDTO : AccountResponseDTO
{

}