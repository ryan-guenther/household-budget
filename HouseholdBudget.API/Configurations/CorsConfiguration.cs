namespace HouseholdBudget.Configurations
{
    /// <summary>
    /// Provides configuration for Cross-Origin Resource Sharing (CORS) policies.
    /// </summary>
    public static class CorsConfiguration
    {
        /// <summary>
        /// Adds and configures CORS policies for the application.
        /// </summary>
        /// <param name="services">The service collection to which the configuration is added.</param>
        /// <returns>The updated service collection.</returns>
        public static IServiceCollection AddCorsConfiguration(this IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });
            });

            return services;
        }
    }
}
