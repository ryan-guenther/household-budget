namespace HouseholdBudget.Configurations
{
    /// <summary>
    /// Provides configuration for logging.
    /// </summary>
    public static class LoggingConfiguration
    {
        /// <summary>
        /// Adds and configures logging services for the application.
        /// </summary>
        /// <param name="services">The service collection to which the configuration is added.</param>
        /// <param name="environment">The hosting environment for the application.</param>
        /// <returns>The updated service collection.</returns>
        public static IServiceCollection AddLoggingConfiguration(this IServiceCollection services, IHostEnvironment environment)
        {
            services.AddLogging(loggingBuilder =>
            {
                if (environment.IsDevelopment())
                {
                    loggingBuilder.AddConsole(options =>
                    {
                        options.IncludeScopes = true; // Include scopes for detailed logs in development
                    });
                }
            });

            return services;
        }
    }
}
