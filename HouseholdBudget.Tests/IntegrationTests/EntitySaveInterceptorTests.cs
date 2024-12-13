using HouseholdBudget.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Moq;
using HouseholdBudget.Domain.Entities;
using HouseholdBudget.Infrastructure.Interfaces;


namespace HouseholdBudget.Tests.IntegrationTests
{
    public class EntitySaveInterceptorTests
    {
        [Fact]
        public void InterceptSave_ShouldSetOwnerUserId_ForBaseEntity()
        {
            // Arrange
            var mockUserContext = new Mock<IUserContext>();
            mockUserContext.Setup(uc => uc.GetNumericUserId()).Returns("123");

            var interceptor = new EntitySaveInterceptor(mockUserContext.Object);

            // Create an in-memory DbContext for testing
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            using var dbContext = new ApplicationDbContext(options, interceptor);

            // Add a test entity
            var testEntity = new Account { Name = "Test Account", Balance = 100 };
            dbContext.Add(testEntity);

            // Act
            dbContext.SaveChanges();

            // Assert
            Assert.Equal("123", testEntity.OwnerUserId);
        }

        [Fact]
        public void InterceptSave_ShouldThrowException_WhenUserIdIsNull()
        {
            // Arrange
            var mockUserContext = new Mock<IUserContext>();
            mockUserContext.Setup(uc => uc.GetNumericUserId()).Returns((string?)null);

            var interceptor = new EntitySaveInterceptor(mockUserContext.Object);

            // Create an in-memory DbContext for testing
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            using var dbContext = new ApplicationDbContext(options, interceptor);

            // Add a test entity
            var testEntity = new Account { Name = "Test Account", Balance = 100 };
            dbContext.Add(testEntity);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => dbContext.SaveChanges());
        }
    }
}