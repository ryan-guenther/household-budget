using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Collections.Generic;
using HouseholdBudget.BusinessLogic;
using Xunit;

namespace HouseholdBudget.Tests.UnitTests;

public class AuthenticationBLTests
{
    private const string ValidJwtKey = "TestSecretKey12345678901234567890"; // 256-bit key
    private const string ShortJwtKey = "ShortKey123"; // Less than 256 bits
    private const string ValidJwtIssuer = "TestIssuer";
    private const string ValidJwtAudience = "TestAudience";
    private const int ValidExpiryMinutes = 30;

    private readonly string _testUserId = "1";
    private readonly string _testUserEmail = "testuser@example.com";
    private readonly List<string> _testUserRoles = new List<string> { "Admin", "User" };

    [Fact]
    public void GenerateJwtToken_ShouldReturnValidToken_WhenInputIsValid()
    {
        // Act
        var token = AuthenticationBL.GenerateJwtToken(
            _testUserId,
            _testUserEmail,
            _testUserRoles,
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
        Assert.Contains(jwtToken.Claims, claim => claim.Type == ClaimTypes.Email && claim.Value == _testUserEmail);
        Assert.Contains(jwtToken.Claims, claim => claim.Type == ClaimTypes.NameIdentifier && claim.Value == _testUserId);
        Assert.Contains(jwtToken.Claims, claim => claim.Type == JwtRegisteredClaimNames.Jti);
        Assert.Contains(jwtToken.Claims, claim => claim.Type == ClaimTypes.Role && claim.Value == "Admin");
        Assert.Contains(jwtToken.Claims, claim => claim.Type == ClaimTypes.Role && claim.Value == "User");
    }

    [Fact]
    public void GenerateJwtToken_ShouldThrowArgumentOutOfRangeException_WhenKeyIsTooShort()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            AuthenticationBL.GenerateJwtToken(
                _testUserId,
                _testUserEmail,
                _testUserRoles,
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
            AuthenticationBL.GenerateJwtToken(_testUserId, _testUserEmail, _testUserRoles, jwtKey, jwtIssuer, jwtAudience, ValidExpiryMinutes));

        Assert.Equal(expectedMessage, exception.Message);
    }

    [Fact]
    public void GenerateJwtToken_ShouldSetExpiryCorrectly()
    {
        // Arrange
        var customExpiryMinutes = 60;

        // Act
        var token = AuthenticationBL.GenerateJwtToken(
            _testUserId,
            _testUserEmail,
            _testUserRoles,
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
