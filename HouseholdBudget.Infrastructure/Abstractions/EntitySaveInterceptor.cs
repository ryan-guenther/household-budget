using System.Security.Claims;

using HouseholdBudget.Domain;
using HouseholdBudget.Infrastructure.Interfaces;

using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace HouseholdBudget.Infrastructure
{
    /// <summary>
    /// Interceptor for handling save operations on entities inheriting from BaseEntity.
    /// </summary>
    public class EntitySaveInterceptor : IEntitySaveInterceptor
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public EntitySaveInterceptor(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Intercepts the save operation and applies the OwnerUserId to new entities.
        /// </summary>
        /// <param name="changeTracker">The change tracker containing tracked entities.</param>
        public void InterceptSave(ChangeTracker changeTracker)
        {
            var userId = GetNumericNameIdentifier();

            if (!string.IsNullOrEmpty(userId))
            {
                foreach (var entry in changeTracker.Entries<BaseEntity>())
                {
                    if (entry.State == EntityState.Added)
                    {
                        entry.Entity.OwnerUserId = userId;
                    }
                }
            }
            else
            {
                throw new InvalidOperationException("Problem with save.");
            }
        }

        /// <summary>
        /// Extracts the numeric NameIdentifier claim from the current user's claims.
        /// </summary>
        /// <returns>The numeric NameIdentifier as a string, or null if not found.</returns>
        private string? GetNumericNameIdentifier()
        {
            var claims = _httpContextAccessor.HttpContext?.User?.Claims;

            if (claims == null) return null;

            return claims
                .Where(c => c.Type == ClaimTypes.NameIdentifier && int.TryParse(c.Value, out _))
                .Select(c => c.Value)
                .FirstOrDefault();
        }
    }
}
