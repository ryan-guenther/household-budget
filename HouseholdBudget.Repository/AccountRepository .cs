using HouseholdBudget.Domain.Entities;
using HouseholdBudget.Infrastructure;
using HouseholdBudget.Infrastructure.Interfaces;

using Microsoft.EntityFrameworkCore;

namespace HouseholdBudget.Repository
{
    public class AccountRepository : IAccountRepository
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IUserContext _userContext;

        public AccountRepository(ApplicationDbContext dbContext,
            IUserContext userContext)
        {
            _dbContext = dbContext;
            _userContext = userContext;
        }

        public IQueryable<Account> GetAll()
        {
            var userId = _userContext.GetNumericUserId();
            var accounts = _dbContext.Accounts.Where(t => t.OwnerUserId == userId);

            return accounts;
        }

        public IQueryable<Account> AdminGetAll()
        {
            var accounts = _dbContext.Accounts;

            return accounts;
        }

        public async Task<Account?> GetByIdAsync(int id)
        {
            var userId = _userContext.GetNumericUserId();
            var account = await _dbContext.Accounts
                .Where(t => t.Id == id
                    && t.OwnerUserId == userId)
                .FirstOrDefaultAsync();

            return account;
        }

        public async Task<Account?> AdminGetByIdAsync(int id)
        {
            var userId = _userContext.GetNumericUserId();
            var account = await _dbContext.Accounts
                .Where(t => t.Id == id)
                .FirstOrDefaultAsync();

            return account;
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
