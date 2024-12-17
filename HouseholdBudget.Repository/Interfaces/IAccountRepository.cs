using HouseholdBudget.Domain.Entities;

namespace HouseholdBudget.Repository
{
    public interface IAccountRepository
    {
        IQueryable<Account> GetAll();
        Task<Account?> GetByIdAsync(int id);
        Task<Account> AddAsync(Account account);
        Task<Account> UpdateAsync(Account account);
        Task DeleteAsync(int id);
    }
}
