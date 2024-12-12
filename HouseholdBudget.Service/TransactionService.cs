using HouseholdBudget.Domain.Entities;
using HouseholdBudget.DTO;
using HouseholdBudget.Repository;
using HouseholdBudget.Service.Interfaces;
using Mapster;
using Microsoft.Extensions.Logging;


public class TransactionService : ITransactionService
{
    private readonly IDbTransactionManager _transactionManager;
    private readonly ITransactionRepository _transactionRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly ILogger<TransactionService> _logger;

    public TransactionService(
        IDbTransactionManager transactionManager,
        ITransactionRepository transactionRepository,
        IAccountRepository accountRepository,
        ILogger<TransactionService> logger)
    {
        _transactionManager = transactionManager;
        _transactionRepository = transactionRepository;
        _accountRepository = accountRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<TransactionDTO>> GetAllAsync()
    {
        try
        {
            var transactions = await _transactionRepository.GetAllAsync();
            return transactions.Adapt<IEnumerable<TransactionDTO>>();  // Automatically map from Transaction to TransactionDTO
        }
        catch (Exception ex)
        {
            string error = "An error occurred while retrieving all transactions";
            _logger.LogError(error, ex);
            throw new ApplicationException($"{error}: {ex.Message}", ex);
        }
    }

    public async Task<TransactionDTO?> GetByIdAsync(int transactionId)
    {
        try
        {
            var transaction = await _transactionRepository.GetByIdAsync(transactionId);
            return transaction?.Adapt<TransactionDTO>();  // Automatically map from Transaction to TransactionDTO
        }
        catch (Exception ex)
        {
            string error = $"An error occurred while retrieving the transaction with ID {transactionId}";
            _logger.LogError(error, ex);
            throw new ApplicationException($"{error}: {ex.Message}", ex);
        }
    }

    public async Task AddAsync(TransactionDTO transactionDto)
    {
        await using (var tm = await _transactionManager.BeginTransactionAsync())
        {
            try
            {
                var account = await _accountRepository.GetByIdAsync(transactionDto.AccountId);
                if (account == null)
                {
                    _logger.LogError($"Account with ID {transactionDto.AccountId} not found.");
                    throw new ArgumentException("Account not found.");
                }

                var transactionEntity = transactionDto.Adapt<Transaction>();
                await _transactionRepository.AddAsync(transactionEntity);

                account = AdjustAccountBalance(account, transactionEntity.Type, transactionEntity.Amount);
                await _accountRepository.UpdateAsync(account);

                await tm.CommitAsync();

                _logger.LogInformation($"Transaction with ID {transactionEntity.Id} added and committed successfully.");
            }
            catch (Exception ex)
            {
                string error = "An error occurred while adding the transaction.";
                _logger.LogError(error, ex);
                await tm.RollbackAsync();
                throw new ApplicationException($"{error}: {ex.Message}", ex);
            }
        }
    }


    // Helper method for adjusting account balance
    private Account AdjustAccountBalance(Account account, TransactionType transactionType, decimal amount)
    {
        if (transactionType == TransactionType.Credit)
        {
            account.Balance += amount;
        }
        else if (transactionType == TransactionType.Expense)
        {
            account.Balance -= amount;
        }

        return account;
    }

    public async Task UpdateAsync(TransactionDTO transactionDto)
    {
        await using (var tm = await _transactionManager.BeginTransactionAsync())
        {
            try
            {
                // Fetch the account using AccountId from the transaction DTO
                var account = await _accountRepository.GetByIdAsync(transactionDto.AccountId);
                if (account == null)
                {
                    _logger.LogError($"Account with ID {transactionDto.AccountId} not found.");
                    throw new ArgumentException("Account not found.");
                }

                // Fetch the original transaction by TransactionId
                var originalTransaction = await _transactionRepository.GetByIdAsync(transactionDto.Id);
                if (originalTransaction == null)
                {
                    _logger.LogError($"Transaction with ID {transactionDto.Id} not found.");
                    throw new ArgumentException("Transaction not found.");
                }

                if (originalTransaction.Account.Id != transactionDto.AccountId)
                {
                    _logger.LogError($"Account ID mismatch for transaction with ID {transactionDto.Id}. Account cannot be changed.");
                    throw new ArgumentException("The account associated with a transaction cannot be changed after creation.");
                }

                var transactionEntity = transactionDto.Adapt<HouseholdBudget.Domain.Entities.Transaction>();  // Map from TransactionDTO to Transaction
                decimal originalTransactionAmount = originalTransaction.Type == TransactionType.Credit ? originalTransaction.Amount : -originalTransaction.Amount;
                decimal newTransactionAmount = transactionEntity.Type == TransactionType.Credit ? transactionEntity.Amount : -transactionEntity.Amount;

                decimal amountOffset = originalTransactionAmount - newTransactionAmount;

                // adjust the properties on the originalTransaction to update
                originalTransaction.Type = transactionEntity.Type;
                originalTransaction.Date = transactionEntity.Date;
                originalTransaction.Amount = transactionEntity.Amount;
                originalTransaction.Description = transactionEntity.Description;

                // Update the account balance inside the transaction scope
                account = AdjustAccountBalance(account, originalTransaction.Type, amountOffset);

                // Update the transaction
                await _transactionRepository.UpdateAsync(originalTransaction);
                await _accountRepository.UpdateAsync(account);

                // Commit the transaction
                await tm.CommitAsync();

                _logger.LogInformation($"Transaction with ID {originalTransaction.Id} updated successfully.");
            }
            catch (Exception ex)
            {
                string error = $"An error occurred while updating transaction with ID {transactionDto.Id}.";
                _logger.LogError(error, ex);
                await tm.RollbackAsync();
                throw new ApplicationException($"{error}: {ex.Message}", ex);
            }
        }
    }

    public async Task DeleteAsync(int transactionId)
    {
        await using (var tm = await _transactionManager.BeginTransactionAsync())
        {
            try
            {
                // Fetch the transaction to be deleted by its ID
                var transactionToDelete = await _transactionRepository.GetByIdAsync(transactionId);
                if (transactionToDelete == null)
                {
                    _logger.LogError($"Transaction with ID {transactionId} not found.");
                    throw new ArgumentException("Transaction not found.");
                }

                // Fetch the associated account for the transaction
                var account = await _accountRepository.GetByIdAsync(transactionToDelete.AccountId);
                if (account == null)
                {
                    _logger.LogError($"Account with ID {transactionToDelete.AccountId} associated with transaction ID {transactionId} not found.");
                    throw new ArgumentException("Account associated with the transaction not found.");
                }

                // Adjust the account balance based on the transaction type
                account = AdjustAccountBalance(account, transactionToDelete.Type, -transactionToDelete.Amount);

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
