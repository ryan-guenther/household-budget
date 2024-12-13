using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace HouseholdBudget.Infrastructure.Interfaces
{
    /// <summary>
    /// Defines the contract for intercepting and modifying save operations on entities.
    /// </summary>
    public interface IEntitySaveInterceptor
    {
        /// <summary>
        /// Intercepts the save operation and applies custom logic to tracked entities.
        /// </summary>
        /// <param name="changeTracker">The change tracker containing tracked entities.</param>
        void InterceptSave(ChangeTracker changeTracker);
    }
}
