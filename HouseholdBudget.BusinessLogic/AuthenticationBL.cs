using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

using Microsoft.IdentityModel.Tokens;

namespace HouseholdBudget.BusinessLogic;

public static class AuthenticationBL
{
    /// <summary>
    /// Generates a JWT token for the specified user.
    /// </summary>
    /// <param name="userId">The user's unique identifier.</param>
    /// <param name="email">The user's email address.</param>
    /// <param name="roles">A list of roles assigned to the user.</param>
    /// <param name="jwtKey">The JWT secret key used for signing the token.</param>
    /// <param name="jwtIssuer">The issuer of the token.</param>
    /// <param name="jwtAudience">The audience of the token.</param>
    /// <param name="expiryMinutes">The token's expiration time in minutes.</param>
    /// <returns>The generated JWT token as a string.</returns>
    /// <exception cref="InvalidOperationException">Thrown if any configuration value is missing or invalid.</exception>
    public static string GenerateJwtToken(string userId, string email, IEnumerable<string> roles, string? jwtKey,
        string? jwtIssuer, string? jwtAudience, int expiryMinutes)
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

        // Base claims
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Email, email)
        };

        // Add roles as claims
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        // Generate JWT
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
