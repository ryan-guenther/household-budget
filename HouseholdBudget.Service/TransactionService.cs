using HouseholdBudget.Domain.Entities;
using HouseholdBudget.DTO.Transaction;
using HouseholdBudget.Repository;
using HouseholdBudget.Service.Interfaces;
using Mapster;
using Microsoft.Extensions.Logging;
using HouseholdBudget.BusinessLogic;
using HouseholdBudget.Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;
using HouseholdBudget.Domain;

public class TransactionService : ITransactionService
{
    private readonly IDbTransactionManager _transactionManager;
    private readonly ITransactionRepository _transactionRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly ILogger<TransactionService> _logger;
    private readonly IUserContext _userContext;

    public TransactionService(
        IDbTransactionManager transactionManager,
        ITransactionRepository transactionRepository,
        IAccountRepository accountRepository,
        ILogger<TransactionService> logger,
        IUserContext userContext)
    {
        _transactionManager = transactionManager;
        _transactionRepository = transactionRepository;
        _accountRepository = accountRepository;
        _logger = logger;
        _userContext = userContext;
    }

    public async Task<IEnumerable<TransactionListResponseDTO>> GetAllAsync()
    {
        IEnumerable<Transaction> transactions;

        try
        {
            transactions = await _transactionRepository.GetAll().ToListAsync();
        }
        catch (Exception ex)
        {
            string error = "An error occurred while retrieving all transactions";
            _logger.LogError(error, ex);
            throw new ApplicationException($"{error}: {ex.Message}", ex);
        }

        return transactions.Adapt<IEnumerable<TransactionListResponseDTO>>();  // Automatically map from Transaction to TransactionDTO
    }

    public async Task<IEnumerable<TransactionListResponseDTO>> AdminGetAllAsync()
    {
        _userContext.DemandRole(Roles.Definitions.Admin);
        IEnumerable<Transaction> transactions;

        try
        {
            transactions = await _transactionRepository.AdminGetAll().ToListAsync();
        }
        catch (Exception ex)
        {
            string error = "An error occurred while retrieving all transactions";
            _logger.LogError(error, ex);
            throw new ApplicationException($"{error}: {ex.Message}", ex);
        }

        return transactions.Adapt<IEnumerable<TransactionListResponseDTO>>();  // Automatically map from Transaction to TransactionDTO
    }

    public async Task<TransactionDetailResponseDTO?> GetByIdAsync(int transactionId)
    {
        Transaction? transaction;

        try
        {
            transaction = await _transactionRepository.GetByIdAsync(transactionId);
        }
        catch (Exception ex)
        {
            string error = $"An error occurred while retrieving the transaction with ID {transactionId}";
            _logger.LogError(error, ex);
            throw new ApplicationException($"{error}: {ex.Message}", ex);
        }

        return transaction?.Adapt<TransactionDetailResponseDTO>();  // Automatically map from Transaction to TransactionDTO
    }

    public async Task<TransactionDetailResponseDTO?> AdminGetByIdAsync(int transactionId)
    {
        _userContext.DemandRole(Roles.Definitions.Admin);
        Transaction? transaction;

        try
        {
            transaction = await _transactionRepository.AdminGetByIdAsync(transactionId);
        }
        catch (Exception ex)
        {
            string error = $"An error occurred while retrieving the transaction with ID {transactionId}";
            _logger.LogError(error, ex);
            throw new ApplicationException($"{error}: {ex.Message}", ex);
        }

        return transaction?.Adapt<TransactionDetailResponseDTO>();  // Automatically map from Transaction to TransactionDTO
    }

    public async Task<TransactionDetailResponseDTO> AddAsync(TransactionCreateRequestDTO transactionDto)
    {   
        Transaction transaction = new();
        var accountId = transactionDto.AccountId;
        var userId = _userContext.GetNumericUserId();
        var isAdmin = _userContext.IsAdmin();

        await using (var tm = await _transactionManager.BeginTransactionAsync())
        {
            try
            {
                Account? account;

                if(isAdmin)
                {
                    account = await _accountRepository.AdminGetByIdAsync(accountId);
                }
                else
                {
                    account = await _accountRepository.GetByIdAsync(accountId);
                }

                if (account == null)
                {
                    _logger.LogError($"Account with ID {accountId} not found.");
                    throw new ArgumentException("Account not found.");
                }

                transaction = transactionDto.Adapt<Transaction>();
                transaction = await _transactionRepository.AddAsync(transaction);

                account.Balance = AccountBL.AdjustAccountBalance(account.Balance, transaction.Type, transaction.Amount);
                await _accountRepository.UpdateAsync(account);

                await tm.CommitAsync();

                _logger.LogInformation($"Transaction with ID {transaction.Id} added and committed successfully.");
            }
            catch (Exception ex)
            {
                string error = "An error occurred while adding the transaction.";
                _logger.LogError(error, ex);
                await tm.RollbackAsync();
                throw new ApplicationException($"{error}: {ex.Message}", ex);
            }
        }

        return transaction.Adapt<TransactionDetailResponseDTO>();  // Automatically map from Transaction to TransactionDTO
    }

    public async Task<TransactionDetailResponseDTO> UpdateAsync(TransactionUpdateRequestDTO transactionDto)
    {
        Transaction? transaction = new();
        var transactionId = transactionDto.Id;
        var userId = _userContext.GetNumericUserId();
        var isAdmin = _userContext.IsAdmin();

        await using (var tm = await _transactionManager.BeginTransactionAsync())
        {
            try
            {
                // Fetch the original transaction by TransactionId
                if (isAdmin)
                {
                    transaction = await _transactionRepository.AdminGetByIdAsync(transactionId);
                }
                else
                {
                    transaction = await _transactionRepository.GetByIdAsync(transactionId);
                }
                
                if (transaction == null)
                {
                    _logger.LogError($"Transaction with ID {transactionId} not found.");
                    throw new ArgumentException("Transaction not found.");
                }

                var accountId = transaction.AccountId;

                // Fetch the account using AccountId
                Account? account;

                if (isAdmin)
                {
                    account = await _accountRepository.AdminGetByIdAsync(accountId);
                }
                else
                {
                    account = await _accountRepository.GetByIdAsync(accountId);
                }

                if (account == null)
                {
                    _logger.LogError($"Account with ID {accountId} not found.");
                    throw new ArgumentException("Account not found.");
                }

                if (transaction.Account.Id != accountId)
                {
                    _logger.LogError($"Account ID mismatch for transaction with ID {accountId}. Account cannot be changed.");
                    throw new ArgumentException("The account associated with a transaction cannot be changed after creation.");
                }

                var updatedTransaction = transactionDto.Adapt<Transaction>();  // Map from TransactionDTO to Transaction
                decimal originalTransactionAmount = transaction.Type == TransactionType.Credit ? transaction.Amount : -transaction.Amount;
                decimal newTransactionAmount = updatedTransaction.Type == TransactionType.Credit ? updatedTransaction.Amount : -updatedTransaction.Amount;

                decimal amountOffset = originalTransactionAmount - newTransactionAmount;

                // adjust the properties on the originalTransaction to update
                transaction.Type = updatedTransaction.Type;
                transaction.Date = updatedTransaction.Date;
                transaction.Amount = updatedTransaction.Amount;
                transaction.Description = updatedTransaction.Description;

                // Update the account balance inside the transaction scope
                account.Balance = AccountBL.AdjustAccountBalance(account.Balance, transaction.Type, amountOffset);

                // Update the transaction
                transaction = await _transactionRepository.UpdateAsync(transaction);
                await _accountRepository.UpdateAsync(account);

                // Commit the transaction
                await tm.CommitAsync();

                _logger.LogInformation($"Transaction with ID {transactionId} updated successfully.");
            }
            catch (Exception ex)
            {
                string error = $"An error occurred while updating transaction with ID {transactionId}.";
                _logger.LogError(error, ex);
                await tm.RollbackAsync();
                throw new ApplicationException($"{error}: {ex.Message}", ex);
            }
        }

        return transaction.Adapt<TransactionDetailResponseDTO>();  // Automatically map from Transaction to TransactionDTO
    }

    public async Task DeleteAsync(int transactionId)
    {
        var userId = _userContext.GetNumericUserId();
        var isAdmin = _userContext.IsAdmin();

        await using (var tm = await _transactionManager.BeginTransactionAsync())
        {
            try
            {
                // Fetch the transaction to be deleted by its ID
                Transaction? transactionToDelete;

                if (isAdmin)
                {
                    transactionToDelete = await _transactionRepository.AdminGetByIdAsync(transactionId);
                }
                else
                {
                    transactionToDelete = await _transactionRepository.GetByIdAsync(transactionId);
                }

                if (transactionToDelete == null)
                {
                    _logger.LogError($"Transaction with ID {transactionId} not found.");
                    throw new ArgumentException("Transaction not found.");
                }

                var accountId = transactionToDelete.AccountId;

                // Fetch the associated account for the transaction
                Account? account;

                if (isAdmin)
                {
                    account = await _accountRepository.AdminGetByIdAsync(accountId);
                }
                else
                {
                    account = await _accountRepository.GetByIdAsync(accountId);
                }

                if (account == null)
                {
                    _logger.LogError($"Account with ID {accountId} not found.");
                    throw new ArgumentException("Account not found.");
                }

                // Adjust the account balance based on the transaction type
                account.Balance = AccountBL.AdjustAccountBalance(account.Balance, transactionToDelete.Type, -transactionToDelete.Amount);

                // Update the account balance
                await _accountRepository.UpdateAsync(account);

                // Delete the transaction
                await _transactionRepository.DeleteAsync(transactionId);

                // Commit the transaction if everything is successful
                await tm.CommitAsync();

                _logger.LogInformation($"Transaction with ID {transactionId} deleted successfully.");
            }
            catch (Exception ex)
            {
                string error = $"An error occurred while deleting the transaction with ID {transactionId}.";
                _logger.LogError(error, ex);
                await tm.RollbackAsync();
                throw new ApplicationException($"{error}: {ex.Message}", ex);
            }
        }
    }
}
