﻿using HouseholdBudget.Service;
using HouseholdBudget.Service.Interfaces;
using HouseholdBudget.Repository;

namespace HouseholdBudget.Configurations
{
    /// <summary>
    /// Provides configuration for application services and repositories.
    /// </summary>
    public static class ServiceConfiguration
    {
        /// <summary>
        /// Adds and configures application services and repositories.
        /// </summary>
        /// <param name="services">The service collection to which the configuration is added.</param>
        /// <returns>The updated service collection.</returns>
        public static IServiceCollection AddServiceConfiguration(this IServiceCollection services)
        {
            // Repositories
            services.AddScoped<IAccountRepository, AccountRepository>();
            services.AddScoped<ITransactionRepository, TransactionRepository>();

            // Services
            services.AddScoped<IAccountService, AccountService>();
            services.AddScoped<ITransactionService, TransactionService>();
            services.AddScoped<IAuthenticationService, AuthenticationService>();

            // Transaction Manager
            services.AddScoped<IDbTransactionManager, DbTransactionManager>();

            return services;
        }
    }
}