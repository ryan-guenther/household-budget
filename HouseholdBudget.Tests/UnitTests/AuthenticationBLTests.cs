using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

using HouseholdBudget.BusinessLogic;

using Microsoft.AspNetCore.Identity;

using Xunit;

namespace HouseholdBudget.Tests.UnitTests;

public class AuthenticationBLTests
{
    private const string ValidJwtKey = "TestSecretKey12345678901234567890"; // 256-bit key
    private const string ShortJwtKey = "ShortKey123"; // Less than 256 bits
    private const string ValidJwtIssuer = "TestIssuer";
    private const string ValidJwtAudience = "TestAudience";
    private const int ValidExpiryMinutes = 30;

    private readonly IdentityUser _testUser = new IdentityUser
    {
        Id = "1",
        Email = "testuser@example.com"
    };

    [Fact]
    public void GenerateJwtToken_ShouldReturnValidToken_WhenInputIsValid()
    {
        // Act
        var token = AuthenticationBL.GenerateJwtToken(
            _testUser,
            ValidJwtKey,
            ValidJwtIssuer,
            ValidJwtAudience,
            ValidExpiryMinutes);

        // Assert
        Assert.NotNull(token);

        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        Assert.Equal(ValidJwtIssuer, jwtToken.Issuer);
        Assert.Equal(ValidJwtAudience, jwtToken.Audiences.First());
        Assert.Contains(jwtToken.Claims, claim => claim.Type == ClaimTypes.Email && claim.Value == _testUser.Email);
        Assert.Contains(jwtToken.Claims, claim => claim.Type == ClaimTypes.NameIdentifier && claim.Value == _testUser.Id);
        Assert.Contains(jwtToken.Claims, claim => claim.Type == JwtRegisteredClaimNames.Jti);
    }

    [Fact]
    public void GenerateJwtToken_ShouldThrowArgumentOutOfRangeException_WhenKeyIsTooShort()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            AuthenticationBL.GenerateJwtToken(
                _testUser,
                ShortJwtKey,
                ValidJwtIssuer,
                ValidJwtAudience,
                ValidExpiryMinutes));

        Assert.Contains("JWT key must be at least 256 bits", exception.Message);
    }

    [Theory]
    [InlineData(null, ValidJwtIssuer, ValidJwtAudience, "Invalid JWT Token configuration in the environment variables or appsettings.")]
    [InlineData("", ValidJwtIssuer, ValidJwtAudience, "Invalid JWT Token configuration in the environment variables or appsettings.")]
    [InlineData(ValidJwtKey, null, ValidJwtAudience, "Invalid JWT Token configuration in the environment variables or appsettings.")]
    [InlineData(ValidJwtKey, "", ValidJwtAudience, "Invalid JWT Token configuration in the environment variables or appsettings.")]
    [InlineData(ValidJwtKey, ValidJwtIssuer, null, "Invalid JWT Token configuration in the environment variables or appsettings.")]
    [InlineData(ValidJwtKey, ValidJwtIssuer, "", "Invalid JWT Token configuration in the environment variables or appsettings.")]
    public void GenerateJwtToken_ShouldThrowInvalidOperationException_WhenConfigurationIsInvalid(
        string jwtKey, string jwtIssuer, string jwtAudience, string expectedMessage)
    {
        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            AuthenticationBL.GenerateJwtToken(_testUser, jwtKey, jwtIssuer, jwtAudience, ValidExpiryMinutes));

        Assert.Equal(expectedMessage, exception.Message);
    }

    [Fact]
    public void GenerateJwtToken_ShouldSetExpiryCorrectly()
    {
        // Arrange
        var customExpiryMinutes = 60;

        // Act
        var token = AuthenticationBL.GenerateJwtToken(
            _testUser,
            ValidJwtKey,
            ValidJwtIssuer,
            ValidJwtAudience,
            customExpiryMinutes);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        var expiry = jwtToken.ValidTo;
        var expectedExpiry = DateTime.UtcNow.AddMinutes(customExpiryMinutes);

        // Allow a small margin for processing time differences
        Assert.True((expiry - expectedExpiry).TotalSeconds < 5);
    }
}
