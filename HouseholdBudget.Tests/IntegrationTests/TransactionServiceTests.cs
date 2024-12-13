using Moq;
using HouseholdBudget.Domain.Entities;
using HouseholdBudget.DTO;
using HouseholdBudget.Repository;
using HouseholdBudget.Infrastructure;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using HouseholdBudget.Infrastructure.Interfaces;

namespace HouseholdBudget.Tests.IntegrationTests
{
    public class TransactionServiceTests
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly Mock<IDbTransactionManager> _mockTransactionManager;
        private readonly TransactionService _transactionService;
        private readonly Mock<IEntitySaveInterceptor> _mockSaveInterceptor;

        public TransactionServiceTests()
        {
            // Setup an in-memory database
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()) // Unique database for each test run
                .Options;

            _mockSaveInterceptor = new Mock<IEntitySaveInterceptor>();

            // Create an instance of ApplicationDbContext
            _dbContext = new ApplicationDbContext(options, _mockSaveInterceptor.Object);

            // Setup the mocked DbTransactionManager
            _mockTransactionManager = new Mock<IDbTransactionManager>();

            // Configure the Mocks for the Transaction Manager
            _mockTransactionManager.Setup(tm => tm.BeginTransactionAsync())
                .ReturnsAsync(_mockTransactionManager.Object);

            _mockTransactionManager.Setup(tm => tm.CommitAsync()).Verifiable();
            _mockTransactionManager.Setup(tm => tm.RollbackAsync()).Verifiable();

            // Initialize TransactionService with the real repositories and mocked transaction manager
            _transactionService = new TransactionService(
                _mockTransactionManager.Object,
                new TransactionRepository(_dbContext),
                new AccountRepository(_dbContext),
                Mock.Of<ILogger<TransactionService>>() // Mocked logger
            );

            // Seed the database with initial data
            SeedDatabase();
        }

        // Seed Account
        const int seedAccountId = 1;
        const string seedAccountName = "Test Account";
        const AccountType seedAccountType = AccountType.Savings;
        const decimal seedAccountBalance = 1000;

        // Seed Credit Transaction
        const int seedCreditTransactionId = 1;
        const decimal seedCreditTransactionAmount = 1100;
        readonly DateTime seedCreditTransactionDate = DateTime.Today;
        const string seedCreditTransactionDescription = "Seeded credit transaction.";
        const TransactionType seedCreditTransactionType = TransactionType.Credit;

        // Seed Debit Transaction
        const int seedDebitTransactionId = 2;
        const decimal seedDebitTransactionAmount = 100;
        const string seedDebitTransactionDescription = "Seeded debit transaction.";
        readonly DateTime seedDebitTransactionDate = DateTime.Today;
        const TransactionType seedDebitTransactionType = TransactionType.Expense;

        int _seedNextAvailableTransactionId;

        private void SeedDatabase()
        {
            var account = new Account(seedAccountId, seedAccountName, seedAccountType, seedAccountBalance);
            var creditTransaction = new Transaction(seedCreditTransactionId, seedCreditTransactionAmount, seedCreditTransactionDate, seedCreditTransactionDescription, seedCreditTransactionType, account);
            var debitTransaction = new Transaction(seedDebitTransactionId, seedDebitTransactionAmount, seedDebitTransactionDate, seedDebitTransactionDescription, seedDebitTransactionType, account);

            _dbContext.Accounts.Add(account);
            _dbContext.Transactions.Add(creditTransaction);
            _dbContext.Transactions.Add(debitTransaction);
            _dbContext.SaveChanges();

            var lastUsedTransactionId = _dbContext.Transactions
                .OrderByDescending(t => t.Id)
                .Select(t => t.Id)
                .FirstOrDefault();

            _seedNextAvailableTransactionId = lastUsedTransactionId + 1;
        }

        /// <summary>
        /// Test will add an additional Debit Transaction to the Seeded Account
        /// Confirms the Transaction Saves Succesfully
        /// Ensures the Account Balance is Updated for the Expense
        /// </summary>
        [Fact]
        public async Task AddTransaction_ShouldBeginAndCommitTransaction()
        {
            // Arrange
            int transactionId = _seedNextAvailableTransactionId;
            decimal transactionAmount = 200;
            var transactionDto = new DTO.Transaction.TransactionCreateRequestDTO { Id = transactionId, Amount = transactionAmount, Type = "Expense", AccountId = seedAccountId };
            decimal expectedAccountBalance = seedAccountBalance - transactionAmount;

            // Act
            await _transactionService.AddAsync(transactionDto);

            // Assert that the transaction is saved to the in-memory DB
            var transaction = await _dbContext.Transactions.FindAsync(transactionId);
            Assert.NotNull(transaction);
            Assert.Equal(transactionAmount, transaction.Amount);

            // Assert that the account is updated to the in-memory DB
            var account = await _dbContext.Accounts.FindAsync(seedAccountId);
            Assert.NotNull(account);
            Assert.Equal(expectedAccountBalance, account.Balance);

            // Verify that transaction methods were called on the mocked manager
            _mockTransactionManager.Verify(tm => tm.BeginTransactionAsync(), Times.Once);
            _mockTransactionManager.Verify(tm => tm.CommitAsync(), Times.Once);
        }

        /// <summary>
        /// Test will attempt to add a transaction to the Database using an Invalid Account
        /// Validates the Account not Found Error is Thrown
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task AddTransaction_ShouldRollbackOnError()
        {
            // Arrange
            var transactionDto = new DTO.Transaction.TransactionCreateRequestDTO { Id = _seedNextAvailableTransactionId, Amount = 100, Type = "Expense", AccountId = 99 }; // Non-existing account

            // Act
            var exception = await Assert.ThrowsAsync<ApplicationException>(() => _transactionService.AddAsync(transactionDto));

            // Assert
            Assert.Contains("Account not found", exception.InnerException?.Message);

            // Verify that transaction methods were called on the mocked manager
            _mockTransactionManager.Verify(tm => tm.RollbackAsync(), Times.Once);
        }

        /// <summary>
        /// Test will attempt to update a seeded transaciton
        /// Switches the Transaction from a Credit to a Expense and changes the Value
        /// Ensure the Update is Succesful to the Transaction
        /// Ensures the Account Balance is updated correctly
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task UpdateTransaction_ShouldUpdateAndCommitTransaction()
        {
            // Arrange
            int transactionId = seedCreditTransactionId;
            decimal transactionAmount = 150;
            var transactionType = TransactionType.Expense;
            var transactionDto = new DTO.Transaction.TransactionUpdateRequestDTO { Id = transactionId, Amount = transactionAmount, Type = transactionType.ToString() };
            var expectedBalance = seedAccountBalance - seedCreditTransactionAmount - transactionAmount;

            // Act
            await _transactionService.UpdateAsync(transactionDto);

            // Assert that the transaction was updated in the in-memory DB
            var transaction = await _dbContext.Transactions.FindAsync(transactionId);
            Assert.NotNull(transaction);
            Assert.Equal(transactionAmount, transaction.Amount);
            Assert.Equal(transactionType, transaction.Type);

            // Verify that the balance adjustments were made to the Account
            var account = await _dbContext.Accounts.FindAsync(seedAccountId);
            Assert.NotNull(account);
            Assert.Equal(expectedBalance, account.Balance);

            // Verify that transaction methods were called on the mocked manager
            _mockTransactionManager.Verify(tm => tm.CommitAsync(), Times.Once);
        }

        /// <summary>
        /// Deletes the Debit Transaction from the Database
        /// Ensures the Transaction is Deleted
        /// Ensures the Account Balance is Updated Succesfully
        /// </summary>
        [Fact]
        public async Task DeleteTransaction_ShouldDeleteAndCommitTransaction()
        {
            // Arrange
            var transactionIdToDelete = seedDebitTransactionId;
            var adjustedAccountBalance = seedAccountBalance + seedDebitTransactionAmount;

            // Act
            await _transactionService.DeleteAsync(transactionIdToDelete);  // Deleting the transaction

            // Assert that the transaction was deleted from the in-memory DB
            var transaction = await _dbContext.Transactions.FindAsync(transactionIdToDelete);
            Assert.Null(transaction);  // Transaction should be deleted

            // Verify that the balance adjustments were made to the Account
            var account = await _dbContext.Accounts.FindAsync(seedAccountId);
            Assert.NotNull(account);  // Account should still exist
            Assert.Equal(adjustedAccountBalance, account.Balance);

            // Verify that transaction methods were called on the mocked manager
            _mockTransactionManager.Verify(tm => tm.BeginTransactionAsync(), Times.Once);
            _mockTransactionManager.Verify(tm => tm.CommitAsync(), Times.Once);
        }

        /// <summary>
        /// Adds two new transactions to the Database
        /// Ensures the GetAllAsync returns all transactions in the database
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetAllTransactions_ShouldReturnTransactions()
        {
            // Arrange
            var transactions = new List<Transaction>
            {
                new Transaction { Id = _seedNextAvailableTransactionId, Amount = 100, Type = TransactionType.Credit, AccountId = seedAccountId },
                new Transaction { Id = _seedNextAvailableTransactionId + 1, Amount = 200, Type = TransactionType.Expense, AccountId = seedAccountId }
            };
            var expectedTransactionCount = transactions.Count() + (_seedNextAvailableTransactionId - 1);
            _dbContext.Transactions.AddRange(transactions);
            _dbContext.SaveChanges();

            // Act
            var result = await _transactionService.GetAllAsync();

            // Assert
            Assert.Equal(expectedTransactionCount, result.Count());
        }

        /// <summary>
        /// Ensures Get Transaction By Id returns if the Transaction Exists
        /// Creates a new Transaction and Fetches it
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetTransactionById_ShouldReturnTransaction_WhenExists()
        {
            // Arrange
            var transaction = new Transaction { Id = _seedNextAvailableTransactionId, Amount = 100, Type = TransactionType.Credit, AccountId = seedAccountId };
            _dbContext.Transactions.Add(transaction);
            _dbContext.SaveChanges();

            // Act
            var result = await _transactionService.GetByIdAsync(_seedNextAvailableTransactionId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(_seedNextAvailableTransactionId, result?.Id);
        }

        /// <summary>
        /// Attempts to Fetch a Transaction by an Invalid Transaction Id
        /// Ensures no result is returned
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetTransactionById_ShouldReturnNull_WhenNotExists()
        {
            // Act
            var result = await _transactionService.GetByIdAsync(99); // Non-existing transaction

            // Assert
            Assert.Null(result);
        }
    }
}
