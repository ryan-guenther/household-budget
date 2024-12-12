using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace HouseholdBudget.Configurations
{
    /// <summary>
    /// Provides configuration for JWT-based authentication.
    /// </summary>
    public static class AuthenticationConfiguration
    {
        /// <summary>
        /// Adds and configures JWT authentication services.
        /// </summary>
        /// <param name="services">The service collection to which the configuration is added.</param>
        /// <param name="configuration">The application's configuration object.</param>
        /// <returns>The updated service collection.</returns>
        public static IServiceCollection AddAuthenticationConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            var jwtKey = Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("JWT_KEY") ?? configuration["Jwt:Key"]);

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = configuration["Jwt:Issuer"],
                    ValidAudience = configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(jwtKey)
                };
            });

            return services;
        }
    }
}
