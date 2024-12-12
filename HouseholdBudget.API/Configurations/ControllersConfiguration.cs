using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;

namespace HouseholdBudget.Configurations
{
    /// <summary>
    /// Provides configuration for controllers and their behavior.
    /// </summary>
    public static class ControllersConfiguration
    {
        /// <summary>
        /// Adds and configures MVC controllers with global authorization policies.
        /// </summary>
        /// <param name="services">The service collection to which the configuration is added.</param>
        /// <returns>The updated service collection.</returns>
        public static IServiceCollection AddControllersConfiguration(this IServiceCollection services)
        {
            services.AddControllers(options =>
            {
                // Apply a global authorization policy that requires all users to be authenticated
                var policy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();
                options.Filters.Add(new AuthorizeFilter(policy));
            });

            return services;
        }
    }
}
