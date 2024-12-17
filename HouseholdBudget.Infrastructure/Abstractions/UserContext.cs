using System.Security.Claims;

using HouseholdBudget.Domain;
using HouseholdBudget.Domain.Interfaces;
using HouseholdBudget.Infrastructure.Interfaces;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

using static HouseholdBudget.Domain.Roles;

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
            return claimsPrincipal?.IsInRole(Roles.Definitions.Admin.Name) ?? false;
        }

        /// <inheritdoc />
        public bool HasRole(RoleDefinition role)
        {
            return _httpContextAccessor.HttpContext?.User?.IsInRole(role.Name) ?? false;
        }

        /// <inheritdoc />
        public void DemandRole(RoleDefinition role)
        {
            if (!HasRole(role))
            {
                throw new UnauthorizedAccessException($"User does not have the required role: {role}");

            }
        }
    }
}
