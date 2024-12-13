using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace HouseholdBudget.BusinessLogic;

public static class AuthenticationBL
{
    /// <summary>
    /// Generates a JWT token for the specified user.
    /// </summary>
    /// <param name="user">The user for whom the token is being generated.</param>
    /// <param name="jwtKey">The JWT secret key used for signing the token.</param>
    /// <param name="jwtIssuer">The issuer of the token.</param>
    /// <param name="jwtAudience">The audience of the token.</param>
    /// <param name="expiryMinutes">The token's expiration time in minutes.</param>
    /// <returns>The generated JWT token as a string.</returns>
    /// <exception cref="InvalidOperationException">Thrown if any configuration value is missing or invalid.</exception>
    public static string GenerateJwtToken(
        IdentityUser user,
        string? jwtKey,
        string? jwtIssuer,
        string? jwtAudience,
        int expiryMinutes)
    {
        if (string.IsNullOrEmpty(jwtKey) || string.IsNullOrWhiteSpace(jwtIssuer) || string.IsNullOrWhiteSpace(jwtAudience))
        {
            throw new InvalidOperationException("Invalid JWT Token configuration in the environment variables or appsettings.");
        }

        // Validate key length (at least 256 bits for HS256)
        if (Encoding.UTF8.GetBytes(jwtKey).Length < 32)
        {
            throw new ArgumentOutOfRangeException(nameof(jwtKey), "JWT key must be at least 256 bits (32 bytes) long.");
        }

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Email, user.Email)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: jwtIssuer,
            audience: jwtAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
