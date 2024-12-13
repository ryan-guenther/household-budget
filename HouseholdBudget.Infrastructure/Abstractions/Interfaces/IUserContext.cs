namespace HouseholdBudget.Infrastructure.Interfaces
{
    /// <summary>
    /// Defines the contract for accessing user-specific context.
    /// </summary>
    public interface IUserContext
    {
        /// <summary>
        /// Retrieves the numeric user ID from the current user's context.
        /// </summary>
        /// <returns>The numeric user ID as a string, or null if not available.</returns>
        string? GetNumericUserId();

        /// <summary>
        /// Determines if the current user has admin privileges.
        /// </summary>
        /// <returns>True if the user is an admin; otherwise, false.</returns>
        bool IsAdmin();
    }
}
