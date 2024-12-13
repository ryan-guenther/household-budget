using HouseholdBudget.Service;
using HouseholdBudget.Service.Interfaces;
using HouseholdBudget.DTO.Authentication;
using HouseholdBudget.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using Microsoft.Extensions.Logging;

namespace HouseholdBudget.Tests.IntegrationTests
{
    public class AuthenticationServiceTests
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IAuthenticationService _authenticationService;
        private readonly Mock<UserManager<IdentityUser>> _mockUserManager;
        private readonly Mock<SignInManager<IdentityUser>> _mockSignInManager;
        private readonly Mock<IConfiguration> _mockConfiguration;

        public AuthenticationServiceTests()
        {
            // Setup in-memory database
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _dbContext = new ApplicationDbContext(options);

            // Mock UserManager and SignInManager
            _mockUserManager = MockUserManager();
            _mockSignInManager = MockSignInManager(_mockUserManager.Object);

            // Mock IConfiguration
            _mockConfiguration = new Mock<IConfiguration>();
            _mockConfiguration.Setup(c => c["Jwt:Key"]).Returns("qrq80kldKmRGTxMCeSBVD0EsHBoXwp7DxybdABjXlLcTPewjVep4OG1M9eZKUvuC1oqwdB0SR6uYmzDxbj8uwpvZLicQAZuOuD8pw8LwqS1L4sZY9tiYqdZjzTfpOqQ24KjGhkp6GZydZvWJHsztiucerfClqyCUPoQQOcw05QAmczLVPoHHX05swBsSwWO8QTvI2QVJuzd0X49rcrDuuMJXSBOyYP3xG3AqrYZlt5yO49gV0dYwQyq2ZFjVS4T0"); // Replace with your test key
            _mockConfiguration.Setup(c => c["Jwt:Issuer"]).Returns("TestIssuer");
            _mockConfiguration.Setup(c => c["Jwt:Audience"]).Returns("TestAudience");
            _mockConfiguration.Setup(c => c["Jwt:ExpiryMinutes"]).Returns("30");

            // Initialize AuthenticationService
            _authenticationService = new AuthenticationService(
                _mockUserManager.Object,
                _mockSignInManager.Object,
                _mockConfiguration.Object,
                Mock.Of<ILogger<AuthenticationService>>() // Mocked logger
                );
        }

        [Fact]
        public async Task RegisterAsync_ShouldRegisterNewUser()
        {
            // Arrange
            var request = new AuthenticationCreateRequestDTO
            {
                Email = "testuser@example.com",
                Password = "Password123!"
            };

            _mockUserManager
                .Setup(um => um.FindByEmailAsync(request.Email))
                .ReturnsAsync((IdentityUser)null);

            _mockUserManager
                .Setup(um => um.CreateAsync(It.IsAny<IdentityUser>(), request.Password))
                .ReturnsAsync(IdentityResult.Success);

            _mockUserManager
                .Setup(um => um.AddToRoleAsync(It.IsAny<IdentityUser>(), "User"))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _authenticationService.RegisterAsync(request);

            // Assert
            Assert.Equal("Registration successful.", result);
            _mockUserManager.Verify(um => um.CreateAsync(It.IsAny<IdentityUser>(), request.Password), Times.Once);
            _mockUserManager.Verify(um => um.AddToRoleAsync(It.IsAny<IdentityUser>(), "User"), Times.Once);
        }

        [Fact]
        public async Task RegisterAsync_ShouldThrowException_WhenUserAlreadyExists()
        {
            // Arrange
            var request = new AuthenticationCreateRequestDTO
            {
                Email = "existinguser@example.com",
                Password = "Password123!"
            };

            _mockUserManager
                .Setup(um => um.FindByEmailAsync(request.Email))
                .ReturnsAsync(new IdentityUser { Email = request.Email });

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _authenticationService.RegisterAsync(request)
            );

            Assert.Equal("A user with this email already exists.", exception.Message);
        }

        [Fact]
        public async Task LoginAsync_ShouldReturnJwtToken_WhenCredentialsAreValid()
        {
            // Arrange
            var request = new AuthenticationLoginRequestDTO
            {
                Email = "validuser@example.com",
                Password = "ValidPassword123!"
            };

            var mockUser = new IdentityUser { Email = request.Email, UserName = request.Email, Id = "1" };

            _mockUserManager
                .Setup(um => um.FindByEmailAsync(request.Email))
                .ReturnsAsync(mockUser);

            _mockSignInManager
                .Setup(sm => sm.CheckPasswordSignInAsync(mockUser, request.Password, false))
                .ReturnsAsync(SignInResult.Success);

            // Act
            var token = await _authenticationService.LoginAsync(request);

            // Assert
            Assert.NotNull(token);
            _mockSignInManager.Verify(sm => sm.CheckPasswordSignInAsync(mockUser, request.Password, false), Times.Once);
        }

        [Fact]
        public async Task LoginAsync_ShouldThrowUnauthorizedAccessException_WhenUserNotFound()
        {
            // Arrange
            var request = new AuthenticationLoginRequestDTO
            {
                Email = "nonexistentuser@example.com",
                Password = "SomePassword123!"
            };

            _mockUserManager
                .Setup(um => um.FindByEmailAsync(request.Email))
                .ReturnsAsync((IdentityUser)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => _authenticationService.LoginAsync(request)
            );

            Assert.Contains("Invalid login attempt", exception.Message);
        }

        [Fact]
        public async Task LoginAsync_ShouldThrowUnauthorizedAccessException_WhenPasswordIsIncorrect()
        {
            // Arrange
            var request = new AuthenticationLoginRequestDTO
            {
                Email = "validuser@example.com",
                Password = "InvalidPassword123!"
            };

            var mockUser = new IdentityUser { Email = request.Email, UserName = request.Email, Id = "1" };

            _mockUserManager
                .Setup(um => um.FindByEmailAsync(request.Email))
                .ReturnsAsync(mockUser);

            _mockSignInManager
                .Setup(sm => sm.CheckPasswordSignInAsync(mockUser, request.Password, false))
                .ReturnsAsync(SignInResult.Failed);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => _authenticationService.LoginAsync(request)
            );

            Assert.Contains("Invalid login attempt", exception.Message);
        }

        private Mock<UserManager<IdentityUser>> MockUserManager()
        {
            var store = new Mock<IUserStore<IdentityUser>>();
            return new Mock<UserManager<IdentityUser>>(
                store.Object, null, null, null, null, null, null, null, null
            );
        }

        private Mock<SignInManager<IdentityUser>> MockSignInManager(UserManager<IdentityUser> userManager)
        {
            var contextAccessor = new Mock<Microsoft.AspNetCore.Http.IHttpContextAccessor>();
            var claimsFactory = new Mock<IUserClaimsPrincipalFactory<IdentityUser>>();
            return new Mock<SignInManager<IdentityUser>>(
                userManager, contextAccessor.Object, claimsFactory.Object, null, null, null, null
            );
        }
    }
}
