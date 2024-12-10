namespace HouseholdBudget.DTO
{
    public class AccountDTO
    {
        public int Id { get; set; }  // Account ID
        public string Name { get; set; } = string.Empty;  // Account name (e.g., "Chequing Account")
        public decimal Balance { get; set; }  // Current balance
        public string Type { get; set; } = string.Empty;  // Account type (e.g., "Chequing", "Savings", etc.)
    }
}
