namespace HouseholdBudget.Domain
{
    /// <summary>
    /// Base class for all entities in the application.
    /// </summary>
    public abstract class BaseEntity
    {
        /// <summary>
        /// Unique identifier for the entity.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Identifier of the user who owns this entity.
        /// </summary>
        public string OwnerUserId { get; set; } = string.Empty;
    }
}
