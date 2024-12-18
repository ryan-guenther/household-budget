using HouseholdBudget.Domain.Entities;
using HouseholdBudget.Infrastructure;
using HouseholdBudget.Infrastructure.Interfaces;

using Microsoft.EntityFrameworkCore;

namespace HouseholdBudget.Repository
{
    public class TransactionRepository : ITransactionRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public TransactionRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IQueryable<Transaction> GetAll()
        {
            return _dbContext.Transactions.AsQueryable();
        }

        public async Task<Transaction?> GetByIdAsync(int id)
        {
            return await _dbContext.Transactions
                .Where(t => t.Id == id)
                .FirstOrDefaultAsync();
        }

        public async Task<Transaction> AddAsync(Transaction transaction)
        {
            await _dbContext.Transactions.AddAsync(transaction);
            await _dbContext.SaveChangesAsync();
            return transaction;
        }

        public async Task<Transaction> UpdateAsync(Transaction transaction)
        {
            _dbContext.Transactions.Update(transaction);
            await _dbContext.SaveChangesAsync();
            return transaction;
        }

        public async Task DeleteAsync(int id)
        {
            var transaction = await _dbContext.Transactions.FindAsync(id);
            if (transaction != null)
            {
                _dbContext.Transactions.Remove(transaction);
                await _dbContext.SaveChangesAsync();
            }
        }
    }
}
