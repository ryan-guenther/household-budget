using System.Security.Claims;

using HouseholdBudget.Domain.Interfaces;
using HouseholdBudget.Infrastructure.Interfaces;

using Microsoft.AspNetCore.Http;

namespace HouseholdBudget.Infrastructure
{
    /// <summary>
    /// Provides access to user-specific context based on the current HTTP context.
    /// </summary>
    public class UserContext : IUserContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserContext(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        /// <inheritdoc />
        public string? GetNumericUserId()
        {
            var claims = _httpContextAccessor.HttpContext?.User?.Claims;

            if (claims == null) return null;

            return claims
                .Where(c => c.Type == ClaimTypes.NameIdentifier && int.TryParse(c.Value, out _))
                .Select(c => c.Value)
                .FirstOrDefault();
        }

        /// <inheritdoc />
        public bool IsAdmin()
        {
            var claimsPrincipal = _httpContextAccessor.HttpContext?.User;
            return claimsPrincipal?.IsInRole("Admin") ?? false;
        }
    }
}
