using HouseholdBudget.Domain.Entities;
using HouseholdBudget.DTO;
using HouseholdBudget.Repository;
using HouseholdBudget.Service.Interfaces;

using Mapster;

namespace HouseholdBudget.Service
{
    public class AccountService : IAccountService
    {
        private readonly IAccountRepository _accountRepository;

        public AccountService(IAccountRepository accountRepository)
        {
            _accountRepository = accountRepository;
        }

        public async Task<IEnumerable<AccountDTO>> GetAllAsync()
        {
            var accounts = await _accountRepository.GetAllAsync();
            return accounts.Adapt<IEnumerable<AccountDTO>>();  // Automatically map from Account to AccountDTO
        }

        public async Task<AccountDTO?> GetByIdAsync(int id)
        {
            var account = await _accountRepository.GetByIdAsync(id);
            return account?.Adapt<AccountDTO>();  // Automatically map from Account to AccountDTO
        }

        public async Task AddAsync(AccountDTO accountDto)
        {
            var account = accountDto.Adapt<Account>();  // Automatically map from AccountDTO to Account
            await _accountRepository.AddAsync(account);
        }

        public async Task UpdateAsync(AccountDTO accountDto)
        {
            var account = accountDto.Adapt<Account>();  // Automatically map from AccountDTO to Account
            await _accountRepository.UpdateAsync(account);
        }

        public async Task DeleteAsync(int id)
        {
            await _accountRepository.DeleteAsync(id);
        }
    }
}
