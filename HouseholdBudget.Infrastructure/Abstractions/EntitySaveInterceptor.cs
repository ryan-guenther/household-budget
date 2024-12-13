using HouseholdBudget.Domain;
using HouseholdBudget.Infrastructure.Interfaces;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace HouseholdBudget.Infrastructure
{
    /// <summary>
    /// Interceptor for handling save operations on entities inheriting from BaseEntity.
    /// </summary>
    public class EntitySaveInterceptor : IEntitySaveInterceptor
    {
        private readonly IUserContext _userContext;

        public EntitySaveInterceptor(IUserContext userContext)
        {
            _userContext = userContext;
        }

        /// <summary>
        /// Intercepts the save operation and applies the OwnerUserId to new entities.
        /// </summary>
        /// <param name="changeTracker">The change tracker containing tracked entities.</param>
        public void InterceptSave(ChangeTracker changeTracker)
        {
            var userId = _userContext.GetNumericUserId();

            if (string.IsNullOrEmpty(userId))
            {
                throw new InvalidOperationException("Authenticated user ID could not be determined.");
            }

            foreach (var entry in changeTracker.Entries<BaseEntity>())
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.OwnerUserId = userId;
                }
            }
        }
    }
}
