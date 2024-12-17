using HouseholdBudget.Domain.Entities;

namespace HouseholdBudget.Repository
{
    public interface ITransactionRepository
    {
        IQueryable<Transaction> GetAll();
        Task<Transaction?> GetByIdAsync(int id);
        Task<Transaction> AddAsync(Transaction transaction);
        Task<Transaction> UpdateAsync(Transaction transaction);
        Task DeleteAsync(int id);
    }
}
