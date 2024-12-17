using HouseholdBudget.Domain.Entities;
using HouseholdBudget.Infrastructure;
using HouseholdBudget.Infrastructure.Interfaces;

using Microsoft.EntityFrameworkCore;

namespace HouseholdBudget.Repository
{
    public class TransactionRepository : ITransactionRepository
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IUserContext _userContext;

        public TransactionRepository(ApplicationDbContext dbContext,
            IUserContext userContext)
        {
            _dbContext = dbContext;
            _userContext = userContext;
        }

        public IQueryable<Transaction> GetAll()
        {
            var userId = _userContext.GetNumericUserId();
            var transactions = _dbContext.Transactions.Where(t => t.OwnerUserId == userId);

            return transactions;
        }

        public IQueryable<Transaction> AdminGetAll()
        {
            var transactions = _dbContext.Transactions;

            return transactions;
        }

        public async Task<Transaction?> GetByIdAsync(int id)
        {
            var userId = _userContext.GetNumericUserId();
            var transaction = await _dbContext.Transactions
                .Where(t => t.Id == id
                    && t.OwnerUserId == userId)
                .FirstOrDefaultAsync();

            return transaction;
        }

        public async Task<Transaction?> AdminGetByIdAsync(int id)
        {
            var userId = _userContext.GetNumericUserId();
            var transaction = await _dbContext.Transactions
                .Where(t => t.Id == id)
                .FirstOrDefaultAsync();

            return transaction;
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
