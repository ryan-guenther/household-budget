using Microsoft.OpenApi.Models;

namespace HouseholdBudget.Configurations
{
    /// <summary>
    /// Provides configuration for Swagger API documentation.
    /// </summary>
    public static class SwaggerConfiguration
    {
        /// <summary>
        /// Adds and configures Swagger services for API documentation.
        /// </summary>
        /// <param name="services">The service collection to which the configuration is added.</param>
        /// <returns>The updated service collection.</returns>
        public static IServiceCollection AddSwaggerConfiguration(this IServiceCollection services)
        {
            services.AddSwaggerGen(options =>
            {
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Enter your valid token."
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });

            return services;
        }
    }
}
