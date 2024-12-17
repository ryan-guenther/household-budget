using HouseholdBudget.Domain.Entities;
using HouseholdBudget.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace HouseholdBudget.Repository
{
    public class AccountRepository : IAccountRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public AccountRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IQueryable<Account> GetAll()
        {
            return _dbContext.Accounts.AsQueryable();
        }

        public async Task<Account?> GetByIdAsync(int id)
        {
            return await _dbContext.Accounts
                .Where(a => a.Id == id)
                .FirstOrDefaultAsync();
        }

        public async Task<Account> AddAsync(Account account)
        {
            await _dbContext.Accounts.AddAsync(account);
            await _dbContext.SaveChangesAsync();
            return account;
        }

        public async Task<Account> UpdateAsync(Account account)
        {
            _dbContext.Accounts.Update(account);
            await _dbContext.SaveChangesAsync();
            return account;
        }

        public async Task DeleteAsync(int id)
        {
            var account = await _dbContext.Accounts.FindAsync(id);
            if (account != null)
            {
                _dbContext.Accounts.Remove(account);
                await _dbContext.SaveChangesAsync();
            }
        }
    }
}
