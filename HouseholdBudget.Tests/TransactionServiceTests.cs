using HouseholdBudget.DTO;
using HouseholdBudget.Service;
using HouseholdBudget.Service.Interfaces;
using HouseholdBudget.Repository;
using Moq;
using FluentAssertions;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using HouseholdBudget.Domain.Entities;

namespace HouseholdBudget.Tests
{
    public class TransactionServiceTests
    {
        private readonly Mock<ITransactionRepository> _transactionRepoMock;
        private readonly Mock<IAccountRepository> _accountRepoMock;
        private readonly ITransactionService _transactionService;

        public TransactionServiceTests()
        {
            // Mocking the ITransactionRepository and IAccountRepository
            _transactionRepoMock = new Mock<ITransactionRepository>();
            _accountRepoMock = new Mock<IAccountRepository>();

            // Injecting the mocked repositories into the TransactionService
            _transactionService = new TransactionService(_transactionRepoMock.Object, _accountRepoMock.Object);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnListOfTransactions()
        {
            // Arrange: Mock the repository's GetAllAsync method to return a list of domain Transaction objects
            var mockTransactions = new List<Transaction>
            {
                new Transaction { Id = 1, Amount = 100, Date = DateTime.Now, Description = "Salary", Type = TransactionType.Credit, AccountId = 1 },
                new Transaction { Id = 2, Amount = 50, Date = DateTime.Now, Description = "Groceries", Type = TransactionType.Expense, AccountId = 1 }
            };

            _transactionRepoMock.Setup(repo => repo.GetAllAsync())
                                .ReturnsAsync(mockTransactions);

            // Act: Call the service method
            var result = await _transactionService.GetAllAsync();

            // Assert: Verify the result
            result.Should().HaveCount(2);  // We expect two transactions
            result.Should().Contain(t => t.Description == "Salary");
            result.Should().Contain(t => t.Description == "Groceries");
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnTransaction_WhenTransactionExists()
        {
            // Arrange: Mock the repository's GetByIdAsync method to return a domain Transaction object
            var mockTransaction = new Transaction { Id = 1, Amount = 100, Date = DateTime.Now, Description = "Salary", Type = TransactionType.Credit, AccountId = 1 };

            _transactionRepoMock.Setup(repo => repo.GetByIdAsync(1))
                                .ReturnsAsync(mockTransaction);

            // Act: Call the service method
            var result = await _transactionService.GetByIdAsync(1);

            // Assert: Verify the result
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(new TransactionDTO { Id = 1, Amount = 100, Date = mockTransaction.Date, Description = "Salary", Type = "Credit", AccountId = 1 });
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenTransactionDoesNotExist()
        {
            // Arrange: Mock the repository's GetByIdAsync method to return null (no transaction found)
            _transactionRepoMock.Setup(repo => repo.GetByIdAsync(It.IsAny<int>()))
                                .ReturnsAsync((Transaction)null);

            // Act: Call the service method
            var result = await _transactionService.GetByIdAsync(999);  // Non-existing transaction ID

            // Assert: Verify the result is null
            result.Should().BeNull();
        }

        [Fact]
        public async Task AddAsync_ShouldAddNewTransaction()
        {
            // Arrange: Create a new transaction DTO
            var newTransaction = new TransactionDTO { Amount = 100, Description = "Salary", Type = "Credit", AccountId = 1 };

            _transactionRepoMock.Setup(repo => repo.AddAsync(It.IsAny<Transaction>()))
                                .Returns(Task.CompletedTask);  // Simulating that the transaction is added

            _accountRepoMock.Setup(repo => repo.GetByIdAsync(1))  // Simulate account exists
                             .ReturnsAsync(new Account { Id = 1, Name = "Chequing", Balance = 1000 });

            // Act: Call the service method to add a new transaction
            await _transactionService.AddAsync(newTransaction);

            // Assert: Verify that the repository's AddAsync was called
            _transactionRepoMock.Verify(repo => repo.AddAsync(It.IsAny<Transaction>()), Times.Once);
            _accountRepoMock.Verify(repo => repo.UpdateAsync(It.IsAny<Account>()), Times.Once);  // Ensure balance update happens
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateExistingTransaction()
        {
            // Arrange: Create an updated transaction DTO
            var updatedTransaction = new TransactionDTO { Id = 1, Amount = 150, Description = "Updated Salary", Type = "Credit", AccountId = 1 };

            _transactionRepoMock.Setup(repo => repo.UpdateAsync(It.IsAny<Transaction>()))
                                .Returns(Task.CompletedTask);  // Simulating that the transaction is updated

            // Act: Call the service method to update the transaction
            await _transactionService.UpdateAsync(updatedTransaction);

            // Assert: Verify that the repository's UpdateAsync was called
            _transactionRepoMock.Verify(repo => repo.UpdateAsync(It.IsAny<Transaction>()), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_ShouldDeleteTransaction_WhenTransactionExists()
        {
            // Arrange: Mock the repository's DeleteAsync method
            _transactionRepoMock.Setup(repo => repo.DeleteAsync(It.IsAny<int>()))
                                .Returns(Task.CompletedTask);  // Simulating that the transaction is deleted

            // Act: Call the service method to delete the transaction
            await _transactionService.DeleteAsync(1);  // Transaction ID to delete

            // Assert: Verify that the repository's DeleteAsync was called
            _transactionRepoMock.Verify(repo => repo.DeleteAsync(1), Times.Once);
        }
    }
}
