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
    public class AccountServiceTests
    {
        private readonly Mock<IAccountRepository> _accountRepoMock;
        private readonly IAccountService _accountService;

        public AccountServiceTests()
        {
            // Mocking the IAccountRepository
            _accountRepoMock = new Mock<IAccountRepository>();

            // Injecting the mocked repository into the AccountService
            _accountService = new AccountService(_accountRepoMock.Object);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnListOfAccounts()
        {
            // Arrange: Mock the repository's GetAllAsync method to return domain Account objects
            var mockAccounts = new List<Account>
            {
                new Account { Id = 1, Name = "Chequing Account", Balance = 1000, Type = AccountType.Chequing },
                new Account { Id = 2, Name = "Savings Account", Balance = 5000, Type = AccountType.Savings }
            };

            _accountRepoMock.Setup(repo => repo.GetAllAsync())
                            .ReturnsAsync(mockAccounts);

            // Act: Call the service method
            var result = await _accountService.GetAllAsync();

            // Assert: Verify the result and expectations
            result.Should().HaveCount(2);  // We expect two accounts
            result.Should().Contain(a => a.Name == "Chequing Account");
            result.Should().Contain(a => a.Name == "Savings Account");
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnAccount_WhenAccountExists()
        {
            // Arrange: Mock the repository's GetByIdAsync method to return a domain Account object
            var mockAccount = new Account { Id = 1, Name = "Chequing Account", Balance = 1000, Type = AccountType.Chequing };

            _accountRepoMock.Setup(repo => repo.GetByIdAsync(1))
                            .ReturnsAsync(mockAccount);

            // Act: Call the service method
            var result = await _accountService.GetByIdAsync(1);

            // Assert: Verify the result
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(new AccountDTO { Id = 1, Name = "Chequing Account", Balance = 1000, Type = "Chequing" });
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenAccountDoesNotExist()
        {
            // Arrange: Mock the repository's GetByIdAsync method to return null (no account found)
            _accountRepoMock.Setup(repo => repo.GetByIdAsync(It.IsAny<int>()))
                            .ReturnsAsync((Account)null);

            // Act: Call the service method
            var result = await _accountService.GetByIdAsync(999);  // Non-existing account ID

            // Assert: Verify the result is null
            result.Should().BeNull();
        }

        [Fact]
        public async Task AddAsync_ShouldAddNewAccount()
        {
            // Arrange: Create a new account DTO
            var newAccount = new AccountDTO { Name = "New Account", Balance = 1000, Type = "Chequing" };

            _accountRepoMock.Setup(repo => repo.AddAsync(It.IsAny<Account>()))
                            .Returns(Task.CompletedTask);  // Simulating that the account is added

            // Act: Call the service method to add a new account
            await _accountService.AddAsync(newAccount);

            // Assert: Verify that the repository's AddAsync was called
            _accountRepoMock.Verify(repo => repo.AddAsync(It.IsAny<Account>()), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateExistingAccount()
        {
            // Arrange: Create an updated account DTO
            var updatedAccount = new AccountDTO { Id = 1, Name = "Updated Account", Balance = 1500, Type = "Savings" };

            _accountRepoMock.Setup(repo => repo.UpdateAsync(It.IsAny<Account>()))
                            .Returns(Task.CompletedTask);  // Simulating that the account is updated

            // Act: Call the service method to update the account
            await _accountService.UpdateAsync(updatedAccount);

            // Assert: Verify that the repository's UpdateAsync was called
            _accountRepoMock.Verify(repo => repo.UpdateAsync(It.IsAny<Account>()), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_ShouldDeleteAccount_WhenAccountExists()
        {
            // Arrange: Mock the repository's DeleteAsync method
            _accountRepoMock.Setup(repo => repo.DeleteAsync(It.IsAny<int>()))
                            .Returns(Task.CompletedTask);  // Simulating that the account is deleted

            // Act: Call the service method to delete the account
            await _accountService.DeleteAsync(1);  // Account ID to delete

            // Assert: Verify that the repository's DeleteAsync was called
            _accountRepoMock.Verify(repo => repo.DeleteAsync(1), Times.Once);
        }
    }
}
