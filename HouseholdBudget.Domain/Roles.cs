using Microsoft.AspNetCore.Identity;

namespace HouseholdBudget.Domain;

/// <summary>
/// Provides constants and predefined roles for managing application-wide roles.
/// </summary>
public static class Roles
{
    /// <summary>
    /// Represents the application's predefined roles with their IDs and names.
    /// </summary>
    public static class Definitions
    {
        /// <summary>
        /// The administrator role definition.
        /// </summary>
        public static readonly RoleDefinition Admin = new RoleDefinition
        {
            Id = 1,
            Name = "Admin"
        };

        /// <summary>
        /// The user role definition.
        /// </summary>
        public static readonly RoleDefinition User = new RoleDefinition
        {
            Id = 2,
            Name = "User"
        };
    }

    /// <summary>
    /// Predefined role objects for IdentityRole seeding.
    /// </summary>
    public static class Predefined
    {
        /// <summary>
        /// The administrator role.
        /// </summary>
        public static readonly IdentityRole AdminRole = CreateRole(Definitions.Admin);

        /// <summary>
        /// The user role.
        /// </summary>
        public static readonly IdentityRole UserRole = CreateRole(Definitions.User);

        /// <summary>
        /// Gets all predefined roles as a collection of <see cref="IdentityRole"/> objects.
        /// </summary>
        public static IEnumerable<IdentityRole> GetAll() => new List<IdentityRole>
        {
            AdminRole,
            UserRole
        };
    }

    /// <summary>
    /// Creates a new IdentityRole instance based on the specified role definition.
    /// </summary>
    /// <param name="roleDefinition">The role definition.</param>
    /// <returns>An <see cref="IdentityRole"/> object.</returns>
    private static IdentityRole CreateRole(RoleDefinition roleDefinition)
    {
        return new IdentityRole
        {
            Id = roleDefinition.Id.ToString(),
            Name = roleDefinition.Name,
            NormalizedName = roleDefinition.Name.ToUpper()
        };
    }

    /// <summary>
    /// Represents the structure of a role definition.
    /// </summary>
    public class RoleDefinition
    {
        /// <summary>
        /// Gets or sets the unique identifier for the role.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the role.
        /// </summary>
        public string Name { get; set; } = string.Empty;
    }
}
