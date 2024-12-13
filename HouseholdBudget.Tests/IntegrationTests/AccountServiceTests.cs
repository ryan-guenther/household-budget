using Moq;
using HouseholdBudget.Service;
using HouseholdBudget.Domain.Entities;
using HouseholdBudget.Repository;
using Microsoft.Extensions.Logging;
using HouseholdBudget.Infrastructure;
using Microsoft.EntityFrameworkCore;
using HouseholdBudget.Infrastructure.Interfaces;

namespace HouseholdBudget.Tests.IntegrationTests
{
    public class AccountServiceTests
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly Mock<IDbTransactionManager> _mockTransactionManager;
        private readonly AccountService _accountService;
        private readonly Mock<IEntitySaveInterceptor> _mockSaveInterceptor;

        public AccountServiceTests()
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
            _accountService = new AccountService(
                _mockTransactionManager.Object,
                new AccountRepository(_dbContext),
                Mock.Of<ILogger<AccountService>>() // Mocked logger
            );

            // Seed the database with initial data
            SeedDatabase();
        }

        // Seed Account
        const int seedAccountId = 1;
        const string seedAccountName = "Test Account";
        const AccountType seedAccountType = AccountType.Savings;
        const decimal seedAccountBalance = 1000;

        int _seedNextAvailableAccountId;

        private void SeedDatabase()
        {
            var account = new Account(seedAccountId, seedAccountName, seedAccountType, seedAccountBalance);

            _dbContext.Accounts.Add(account);
            _dbContext.SaveChanges();

            var lastUsedAccountId = _dbContext.Accounts
                .OrderByDescending(t => t.Id)
                .Select(t => t.Id)
                .FirstOrDefault();

            _seedNextAvailableAccountId = lastUsedAccountId + 1;
        }

        [Fact]
        public async Task AddAccount_ShouldBeginAndCommitTransaction()
        {
            // Arrange
            int accountId = _seedNextAvailableAccountId;
            decimal startingBalance = -200;
            string accountName = "Test Account add.";
            AccountType accountType = AccountType.CreditCard;
            var accountDto = new DTO.Account.AccountCreateRequestDTO() { Id = accountId, Name = accountName, Type = accountType.ToString(), Balance = startingBalance };

            // Act
            await _accountService.AddAsync(accountDto);

            // Assert that the account is saved to the in-memory DB
            var account = await _dbContext.Accounts.FindAsync(accountId);
            Assert.NotNull(account);
            Assert.Equal(startingBalance, account.Balance);
            Assert.Equal(accountType, account.Type);
            Assert.Equal(accountName, account.Name);

            // Verify that transaction methods were called on the mocked manager
            _mockTransactionManager.Verify(tm => tm.BeginTransactionAsync(), Times.Once);
            _mockTransactionManager.Verify(tm => tm.CommitAsync(), Times.Once);
        }

        [Fact]
        public async Task AddAccount_ShouldRollbackOnError()
        {
            // Arrange
            var accountDto = new DTO.Account.AccountCreateRequestDTO { Id = seedAccountId, Balance = 100, Type = "Savings" }; // Existing Account Id

            // Act
            var exception = await Assert.ThrowsAsync<ApplicationException>(() => _accountService.AddAsync(accountDto));

            // Assert
            Assert.Contains("An error occurred while adding the account", exception.Message);

            // Verify that transaction methods were called on the mocked manager
            _mockTransactionManager.Verify(tm => tm.RollbackAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateAccount_ShouldUpdateAndCommitTransaction()
        {
            // Arrange
            int accountId = seedAccountId;
            decimal accountBalance = (decimal)375.12;
            string accountName = "New account name.";
            var accountType = AccountType.Chequing;

            var accountDto = new DTO.Account.AccountUpdateRequestDTO { Id = accountId, Balance = accountBalance, Name = accountName, Type = accountType.ToString() };

            // Act
            await _accountService.UpdateAsync(accountDto);

            // Assert that the account was updated in the in-memory DB
            var account = await _dbContext.Accounts.FindAsync(accountId);
            Assert.NotNull(account);
            Assert.Equal(accountBalance, account.Balance);
            Assert.Equal(accountType, account.Type);
            Assert.Equal(accountName, account.Name);

            // Verify that transaction methods were called on the mocked manager
            _mockTransactionManager.Verify(tm => tm.CommitAsync(), Times.Once);
        }

        [Fact]
        public async Task DeleteAccount_ShouldDeleteAndCommitTransaction()
        {
            // Arrange
            var accountIdToDelete = seedAccountId;

            // Act
            await _accountService.DeleteAsync(accountIdToDelete);  // Deleting the account

            // Assert that the account was deleted from the in-memory DB
            var account = await _dbContext.Accounts.FindAsync(accountIdToDelete);
            Assert.Null(account);  // account should be deleted

            // Verify that transaction methods were called on the mocked manager
            _mockTransactionManager.Verify(tm => tm.BeginTransactionAsync(), Times.Once);
            _mockTransactionManager.Verify(tm => tm.CommitAsync(), Times.Once);
        }

        [Fact]
        public async Task GetAllAccounts_ShouldReturnAccounts()
        {
            // Arrange
            var accounts = new List<Account>
            {
                new Account { Id = _seedNextAvailableAccountId, Name = "Account 1", Type = AccountType.Chequing },
                new Account { Id = _seedNextAvailableAccountId + 1, Name = "Account 2", Type = AccountType.Savings }
            };
            var expectedAccountsCount = accounts.Count() + (_seedNextAvailableAccountId - 1);
            _dbContext.Accounts.AddRange(accounts);
            _dbContext.SaveChanges();

            // Act
            var result = await _accountService.GetAllAsync();

            // Assert
            Assert.Equal(expectedAccountsCount, result.Count());
        }

        [Fact]
        public async Task GetAccountById_ShouldReturnAccount_WhenExists()
        {
            // Arrange
            var account = new Account { Id = _seedNextAvailableAccountId, Balance = 100, Type = AccountType.Savings };
            _dbContext.Accounts.Add(account);
            _dbContext.SaveChanges();

            // Act
            var result = await _accountService.GetByIdAsync(_seedNextAvailableAccountId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(_seedNextAvailableAccountId, result?.Id);
        }

        [Fact]
        public async Task GetAccountById_ShouldReturnNull_WhenNotExists()
        {
            // Act
            var result = await _accountService.GetByIdAsync(99); // Non-existing account

            // Assert
            Assert.Null(result);
        }
    }
}
