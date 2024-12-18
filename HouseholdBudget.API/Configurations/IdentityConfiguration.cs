using Microsoft.AspNetCore.Identity;
using HouseholdBudget.Infrastructure;

namespace HouseholdBudget.Configurations
{
    /// <summary>
    /// Provides configuration for ASP.NET Core Identity.
    /// </summary>
    public static class IdentityConfiguration
    {
        /// <summary>
        /// Adds and configures ASP.NET Core Identity services.
        /// </summary>
        /// <param name="services">The service collection to which the configuration is added.</param>
        /// <returns>The updated service collection.</returns>
        public static IServiceCollection AddIdentityConfiguration(this IServiceCollection services)
        {
            services.AddIdentity<IdentityUser, IdentityRole>(options =>
            {
                options.ClaimsIdentity.RoleClaimType = "role"; // Ensure "role" is used as the claim type
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

            return services;
        }
    }
}
