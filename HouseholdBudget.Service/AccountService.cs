using HouseholdBudget.Domain;
using HouseholdBudget.Domain.Entities;
using HouseholdBudget.DTO.Account;
using HouseholdBudget.Infrastructure.Interfaces;
using HouseholdBudget.Repository;
using HouseholdBudget.Service.Interfaces;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;


namespace HouseholdBudget.Service
{
    public class AccountService : IAccountService
    {
        private readonly IDbTransactionManager _transactionManager;
        private readonly IAccountRepository _accountRepository;
        private readonly ILogger<AccountService> _logger;
        private readonly IUserContext _userContext;

        public AccountService(
            IDbTransactionManager transactionManager,
            IAccountRepository accountRepository,
            ILogger<AccountService> logger,
            IUserContext userContext)
        {
            _transactionManager = transactionManager;
            _accountRepository = accountRepository;
            _logger = logger;
            _userContext = userContext;
        }

        public async Task<IEnumerable<AccountListResponseDTO>> GetAllAsync()
        {
            IEnumerable<Account> accounts;
            var userId = _userContext.GetGuidUserId();

            try
            {
                accounts = await _accountRepository.GetAll()
                    .Where(a => a.OwnerUserId == userId) // Filter by user ID
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                string error = "An error occurred while retrieving all accounts";
                _logger.LogError(error, ex);
                throw new ApplicationException($"{error}: {ex.Message}", ex);
            }

            return accounts.Adapt<IEnumerable<AccountListResponseDTO>>(); // Automatically map from Account to AccountListResponseDTO
        }

        public async Task<IEnumerable<AccountListResponseDTO>> AdminGetAllAsync()
        {
            _userContext.DemandRole(Roles.Definitions.Admin);
            IEnumerable<Account> accounts;

            try
            {
                accounts = await _accountRepository.GetAll().ToListAsync();
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
            var userId = _userContext.GetGuidUserId();
            var isAdmin = _userContext.IsAdmin();

            try
            {
                account = await _accountRepository.GetByIdAsync(accountId);

                if (account == null)
                {
                    _logger.LogWarning($"Account with ID {accountId} was not found.");
                    return null;  // Account not found
                }

                // Enforce ownership check
                if (!isAdmin && account.OwnerUserId != userId)
                {
                    _logger.LogWarning($"Account with ID {accountId} not found for user {userId}.");
                    return null;
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

        public async Task<AccountDetailResponseDTO?> AdminGetByIdAsync(int accountId)
        {
            _userContext.DemandRole(Roles.Definitions.Admin);
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
            var accountId = accountDto.Id;
            var userId = _userContext.GetGuidUserId();
            var isAdmin = _userContext.IsAdmin();

            await using (var tm = await _transactionManager.BeginTransactionAsync())
            {
                try
                {
                    // Fetch the account
                    account = await _accountRepository.GetByIdAsync(accountId);
                    if (account == null)
                    {
                        _logger.LogError($"Account with ID {accountId} not found.");
                        throw new ArgumentException("Account not found.");
                    }

                    // Enforce ownership or admin access
                    if (!isAdmin && account.OwnerUserId != userId)
                    {
                        _logger.LogWarning($"User {userId} attempted to update account {accountId} without ownership.");
                        throw new UnauthorizedAccessException("You do not have permission to update this account.");
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

                    _logger.LogInformation($"Account with ID {accountId} updated successfully.");
                }
                catch (Exception ex)
                {
                    string error = $"An error occurred while updating account with ID {accountId}.";
                    _logger.LogError(error, ex);
                    await tm.RollbackAsync();
                    throw new ApplicationException($"{error}: {ex.Message}", ex);
                }
            }

            return account.Adapt<AccountDetailResponseDTO>();  // Automatically map from Account to AccountDetailResponseDTO
        }

        public async Task DeleteAsync(int accountId)
        {
            var userId = _userContext.GetGuidUserId();
            var isAdmin = _userContext.IsAdmin();

            await using (var tm = await _transactionManager.BeginTransactionAsync())
            {
                try
                {
                    // Fetch the account
                    var account = await _accountRepository.GetByIdAsync(accountId);
                    if (account == null)
                    {
                        _logger.LogError($"Account with ID {accountId} not found.");
                        throw new ArgumentException("Account not found.");
                    }

                    // Enforce ownership or admin access
                    if (!isAdmin && account.OwnerUserId != userId)
                    {
                        _logger.LogWarning($"User {userId} attempted to update account {accountId} without ownership.");
                        throw new UnauthorizedAccessException("You do not have permission to update this account.");
                    }

                    if (account == null)
                    {
                        _logger.LogError($"Account with ID {accountId} not found.");
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
