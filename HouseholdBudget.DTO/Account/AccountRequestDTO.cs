namespace HouseholdBudget.DTO.Account;
public abstract class AccountRequestDTO : BaseDTO
{
    public string Name { get; set; } = string.Empty;  // Account name (e.g., "Chequing Account")
    public decimal Balance { get; set; }  // Current balance
    public string Type { get; set; } = string.Empty;  // Account type (e.g., "Chequing", "Savings", etc.)
}

public sealed class AccountCreateRequestDTO : AccountRequestDTO
{

}

public sealed class AccountUpdateRequestDTO : AccountRequestDTO
{

}