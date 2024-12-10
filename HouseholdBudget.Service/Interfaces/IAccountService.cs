using HouseholdBudget.DTO;

namespace HouseholdBudget.Service.Interfaces
{
    public interface IAccountService
    {
        Task<IEnumerable<AccountDTO>> GetAllAsync();
        Task<AccountDTO?> GetByIdAsync(int id);
        Task AddAsync(AccountDTO accountDto);
        Task UpdateAsync(AccountDTO accountDto);
        Task DeleteAsync(int id);
    }
}
