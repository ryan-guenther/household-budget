using HouseholdBudget.Domain.Entities;
using HouseholdBudget.DTO.Transaction;
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

    public async Task<IEnumerable<TransactionListResponseDTO>> GetAllAsync()
    {
        IEnumerable<Transaction> transactions;

        try
        {
            transactions = await _transactionRepository.GetAllAsync();
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

    public async Task<TransactionDetailResponseDTO> AddAsync(TransactionCreateRequestDTO transactionDto)
    {   
        Transaction transaction = new();

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

                transaction = transactionDto.Adapt<Transaction>();
                transaction = await _transactionRepository.AddAsync(transaction);

                account = AdjustAccountBalance(account, transaction.Type, transaction.Amount);
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

    public async Task<TransactionDetailResponseDTO> UpdateAsync(TransactionUpdateRequestDTO transactionDto)
    {
        Transaction? transaction = new();

        await using (var tm = await _transactionManager.BeginTransactionAsync())
        {
            try
            {
                // Fetch the account using AccountId from the transaction DTO
                var account = await _accountRepository.GetByIdAsync(transactionDto.Id);
                if (account == null)
                {
                    _logger.LogError($"Account with ID {transactionDto.Id} not found.");
                    throw new ArgumentException("Account not found.");
                }

                // Fetch the original transaction by TransactionId
                transaction = await _transactionRepository.GetByIdAsync(transactionDto.Id);
                if (transaction == null)
                {
                    _logger.LogError($"Transaction with ID {transactionDto.Id} not found.");
                    throw new ArgumentException("Transaction not found.");
                }

                if (transaction.Account.Id != transactionDto.Id)
                {
                    _logger.LogError($"Account ID mismatch for transaction with ID {transactionDto.Id}. Account cannot be changed.");
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
                account = AdjustAccountBalance(account, transaction.Type, amountOffset);

                // Update the transaction
                transaction = await _transactionRepository.UpdateAsync(transaction);
                await _accountRepository.UpdateAsync(account);

                // Commit the transaction
                await tm.CommitAsync();

                _logger.LogInformation($"Transaction with ID {transaction.Id} updated successfully.");
            }
            catch (Exception ex)
            {
                string error = $"An error occurred while updating transaction with ID {transactionDto.Id}.";
                _logger.LogError(error, ex);
                await tm.RollbackAsync();
                throw new ApplicationException($"{error}: {ex.Message}", ex);
            }
        }

        return transaction.Adapt<TransactionDetailResponseDTO>();  // Automatically map from Transaction to TransactionDTO
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
