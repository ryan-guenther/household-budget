namespace HouseholdBudget.DTO;

public class ExpenseDTO
{
    public int Id { get; set; } 
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    public string? Notes { get; set; }
}
