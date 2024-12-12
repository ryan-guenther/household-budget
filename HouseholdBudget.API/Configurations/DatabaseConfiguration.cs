using Microsoft.EntityFrameworkCore;
using HouseholdBudget.Infrastructure;

namespace HouseholdBudget.Configurations
{
    /// <summary>
    /// Provides configuration for the application's database context.
    /// </summary>
    public static class DatabaseConfiguration
    {
        /// <summary>
        /// Adds and configures the application's database context.
        /// </summary>
        /// <param name="services">The service collection to which the configuration is added.</param>
        /// <param name="configuration">The application's configuration object.</param>
        /// <param name="environment">The hosting environment for the application.</param>
        /// <returns>The updated service collection.</returns>
        public static IServiceCollection AddDatabaseConfiguration(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
        {
            var connectionString = Environment.GetEnvironmentVariable("DATABASE_CONNECTION_STRING")
                ?? configuration.GetConnectionString("DefaultConnection");

            services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
            {
                var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
                options.UseSqlServer(connectionString)
                       .UseLoggerFactory(loggerFactory);

                if (environment.IsDevelopment())
                {
                    options.EnableSensitiveDataLogging(); // Enable detailed logging in development.
                }
            });

            return services;
        }
    }
}
