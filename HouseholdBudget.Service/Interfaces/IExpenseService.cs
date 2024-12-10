using HouseholdBudget.DTO;

namespace HouseholdBudget.Service.Interfaces
{
    public interface IExpenseService
    {
        Task<IEnumerable<ExpenseDTO>> GetAllAsync();
        Task<ExpenseDTO?> GetByIdAsync(int id);
        Task AddAsync(ExpenseDTO expense);
        Task UpdateAsync(ExpenseDTO expense);
        Task DeleteAsync(int id);
    }
}
