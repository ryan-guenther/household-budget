using HouseholdBudget.Domain.Entities;
using HouseholdBudget.DTO.Account;
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

        public async Task<IEnumerable<AccountListResponseDTO>> GetAllAsync()
        {
            IEnumerable<Account> accounts;

            try
            {
                accounts = await _accountRepository.GetAllAsync();
            }
            catch (Exception ex)
            {
                string error = "An error occurred while retrieving all accounts";
                _logger.LogError(error, ex);
                throw new ApplicationException($"{error}: {ex.Message}", ex);
            }

            return accounts.Adapt<IEnumerable<AccountListResponseDTO>>(); // Automatically map from Account to AccountListResponseDTO
        }

        public async Task<AccountDetailResponseDTO?> GetByIdAsync(int accountId)
        {
            Account? account;

            try
            {
                account = await _accountRepository.GetByIdAsync(accountId);
                if (account == null)
                {
                    _logger.LogWarning($"Account with ID {accountId} was not found.");
                    return null;  // Account not found
                }
            }
            catch (Exception ex)
            {
                string error = $"An error occurred while retrieving the account with ID {accountId}.";
                _logger.LogError(error, ex);
                throw new ApplicationException($"{error} : {ex.Message}", ex);
            }

            return account.Adapt<AccountDetailResponseDTO>();  // Automatically map from Account to AccountDetailResponseDTO
        }

        public async Task<AccountDetailResponseDTO> AddAsync(AccountCreateRequestDTO accountDto)
        {
            Account account;

            await using (var tm = await _transactionManager.BeginTransactionAsync())
            {
                try
                {
                    account = accountDto.Adapt<Account>();  // Automatically map from AccountCreateRequestDTO to Account

                    account = await _accountRepository.AddAsync(account);

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

            return account.Adapt<AccountDetailResponseDTO>();  // Automatically map from Account to AccountDetailResponseDTO
        }

        public async Task<AccountDetailResponseDTO> UpdateAsync(AccountUpdateRequestDTO accountDto)
        {
            Account? account;
            await using (var tm = await _transactionManager.BeginTransactionAsync())
            {
                try
                {
                    account = await _accountRepository.GetByIdAsync(accountDto.Id);
                    if(account == null)
                    {
                        _logger.LogError($"Account with ID {accountDto.Id} not found.");
                        throw new ArgumentException("Account not found.");
                    }

                    var updatedAccount = accountDto.Adapt<Account>();  // Automatically map from AccountUpdateRequestDTO to Account

                    // adjust the properties on the originalAccount to update
                    account.Name = updatedAccount.Name;
                    account.Balance = updatedAccount.Balance;
                    account.Type = updatedAccount.Type;

                    // Update the Account
                    account = await _accountRepository.UpdateAsync(account);

                    // Commit the transaction
                    await tm.CommitAsync();

                    _logger.LogInformation($"Account with ID {account.Id} updated successfully.");
                }
                catch (Exception ex)
                {
                    string error = $"An error occurred while updating account with ID {accountDto.Id}.";
                    _logger.LogError(error, ex);
                    await tm.RollbackAsync();
                    throw new ApplicationException($"{error}: {ex.Message}", ex);
                }
            }
            
            return account.Adapt<AccountDetailResponseDTO>();  // Automatically map from Account to AccountDetailResponseDTO
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
