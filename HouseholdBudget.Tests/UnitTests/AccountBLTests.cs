using HouseholdBudget.BusinessLogic;
using HouseholdBudget.Domain.Entities;

namespace HouseholdBudget.Tests.BusinessLogic
{
    public class AccountBLTests
    {
        [Fact]
        public void AdjustAccountBalance_CreditTransaction_IncreasesBalance()
        {
            // Arrange
            var accountBalance = 100m;
            var transactionType = TransactionType.Credit;
            var amount = 50m;

            // Act
            var result = AccountBL.AdjustAccountBalance(accountBalance, transactionType, amount);

            // Assert
            Assert.Equal(150m, result);
        }

        [Fact]
        public void AdjustAccountBalance_ExpenseTransaction_DecreasesBalance()
        {
            // Arrange
            var accountBalance = 100m;
            var transactionType = TransactionType.Expense;
            var amount = 50m;

            // Act
            var result = AccountBL.AdjustAccountBalance(accountBalance, transactionType, amount);

            // Assert
            Assert.Equal(50m, result);
        }

        [Fact]
        public void AdjustAccountBalance_ZeroAmount_DoesNotChangeBalance()
        {
            // Arrange
            var accountBalance = 100m;
            var transactionType = TransactionType.Credit;
            var amount = 0m;

            // Act
            var result = AccountBL.AdjustAccountBalance(accountBalance, transactionType, amount);

            // Assert
            Assert.Equal(100m, result);
        }

        [Fact]
        public void AdjustAccountBalance_NegativeAmount_UpdatesBalanceCorrectly()
        {
            // Arrange
            var accountBalance = 100m;
            var transactionType = TransactionType.Credit;
            var amount = -50m;

            // Act
            var result = AccountBL.AdjustAccountBalance(accountBalance, transactionType, amount);

            // Assert
            Assert.Equal(50m, result);
        }

        [Fact]
        public void AdjustAccountBalance_InvalidTransactionType_ThrowsArgumentException()
        {
            // Arrange
            var accountBalance = 100m;
            var transactionType = (TransactionType)999; // Invalid transaction type
            var amount = 50m;

            // Act & Assert
            Assert.Throws<ArgumentException>(() => AccountBL.AdjustAccountBalance(accountBalance, transactionType, amount));
        }
    }
}
