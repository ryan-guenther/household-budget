using HouseholdBudget.Domain.Entities;

namespace HouseholdBudget.Repository
{
    public interface IAccountRepository
    {
        Task<IEnumerable<Account>> GetAllAsync();
        Task<Account?> GetByIdAsync(int id);
        Task<Account> AddAsync(Account account);
        Task<Account> UpdateAsync(Account account);
        Task DeleteAsync(int id);
    }
}
