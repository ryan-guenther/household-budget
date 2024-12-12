using HouseholdBudget.Domain.Entities;
using HouseholdBudget.DTO;
using HouseholdBudget.Repository;
using HouseholdBudget.Service.Interfaces;
using Mapster;
using Microsoft.Extensions.Logging;


namespace HouseholdBudget.Service
{
    public class AccountService : IAccountService
    {
        private readonly IDbTransactionManager _transactionManager;
        private readonly IAccountRepository _accountRepository;
        private readonly ILogger<AccountService> _logger;

        public AccountService(
            IDbTransactionManager transactionManager,
            IAccountRepository accountRepository,
            ILogger<AccountService> logger)
        {
            _transactionManager = transactionManager;
            _accountRepository = accountRepository;
            _logger = logger;
        }

        public async Task<IEnumerable<AccountDTO>> GetAllAsync()
        {
            try
            {
                var accounts = await _accountRepository.GetAllAsync();
                return accounts.Adapt<IEnumerable<AccountDTO>>(); // Automatically map from Account to AccountDTO
            }
            catch (Exception ex)
            {
                string error = "An error occurred while retrieving all accounts";
                _logger.LogError(error, ex);
                throw new ApplicationException($"{error}: {ex.Message}", ex);
            }
        }

        public async Task<AccountDTO?> GetByIdAsync(int accountId)
        {
            try
            {
                var account = await _accountRepository.GetByIdAsync(accountId);
                if (account == null)
                {
                    _logger.LogWarning($"Account with ID {accountId} was not found.");
                    return null;  // Account not found
                }
                return account.Adapt<AccountDTO>();  // Automatically map from Account to AccountDTO
            }
            catch (Exception ex)
            {
                string error = $"An error occurred while retrieving the account with ID {accountId}.";
                _logger.LogError(error, ex);
                throw new ApplicationException($"{error} : {ex.Message}", ex);
            }
        }

        public async Task AddAsync(AccountDTO accountDto)
        {
            await using (var tm = await _transactionManager.BeginTransactionAsync())
            {
                try
                {
                    var account = accountDto.Adapt<Account>();  // Automatically map from AccountDTO to Account
                    
                    await _accountRepository.AddAsync(account);

                    await tm.CommitAsync();

                    _logger.LogInformation($"Account with ID {account.Id} added successfully.");
                }
                catch (Exception ex)
                {
                    string error = "An error occurred while adding the account.";
                    _logger.LogError(error, ex);
                    await tm.RollbackAsync();
                    throw new ApplicationException($"{error}: {ex.Message}", ex);
                }
            }
        }

        public async Task UpdateAsync(AccountDTO accountDto)
        {
            await using (var tm = await _transactionManager.BeginTransactionAsync())
            {
                try
                {
                    var originalAccount = await _accountRepository.GetByIdAsync(accountDto.Id);
                    if(originalAccount == null)
                    {
                        _logger.LogError($"Account with ID {accountDto.Id} not found.");
                        throw new ArgumentException("Account not found.");
                    }

                    var account = accountDto.Adapt<Account>();  // Automatically map from AccountDTO to Account

                    // adjust the properties on the oriignalAccount to update
                    originalAccount.Name = account.Name;
                    originalAccount.Balance = account.Balance;
                    originalAccount.Type = account.Type;

                    // Update the Account
                    await _accountRepository.UpdateAsync(originalAccount);

                    // Commit the transaction
                    await tm.CommitAsync();

                    _logger.LogInformation($"Account with ID {originalAccount.Id} updated successfully.");
                }
                catch (Exception ex)
                {
                    string error = $"An error occurred while updating account with ID {accountDto.Id}.";
                    _logger.LogError(error, ex);
                    await tm.RollbackAsync();
                    throw new ApplicationException($"{error}: {ex.Message}", ex);
                }
            }       
        }

        public async Task DeleteAsync(int accountId)
        {
            await using (var tm = await _transactionManager.BeginTransactionAsync())
            {
                try
                {
                    // Fetch the account to be deleted by its ID
                    var transactionToDelete = await _accountRepository.GetByIdAsync(accountId);
                    if (transactionToDelete == null)
                    {
                        _logger.LogError($"Account with ID {accountId} not found.");
                        throw new ArgumentException("Account not found.");
                    }

                    // Delete the Account
                    await _accountRepository.DeleteAsync(accountId);

                    // Commit the transaction if everything is successful
                    await tm.CommitAsync();

                    _logger.LogInformation($"Account with ID {accountId} deleted successfully.");
                }
                catch (Exception ex)
                {
                    string error = $"An error occurred while deleting the account with ID {accountId}.";
                    _logger.LogError(error, ex);
                    await tm.RollbackAsync();
                    throw new ApplicationException($"{error}: {ex.Message}", ex);
                }
            }
        }
    }
}
