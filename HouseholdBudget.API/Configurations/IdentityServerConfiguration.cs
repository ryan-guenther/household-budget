using Duende.IdentityServer.Models;

using System.Collections.Generic;

namespace HouseholdBudget.Configurations
{
    /// <summary>
    /// Provides configuration for IdentityServer, including resources, API scopes, and clients.
    /// </summary>
    public static class IdentityServerConfiguration
    {
        /// <summary>
        /// Gets the Identity resources, which represent claims available to the authenticated user.
        /// </summary>
        public static IEnumerable<IdentityResource> IdentityResources =>
            new List<IdentityResource>
            {
                new IdentityResources.OpenId(), // OpenID Connect scope
                new IdentityResources.Profile(), // User profile information
                new IdentityResource("roles", new[] { "role" }) // Add roles as an identity resource
            };

        /// <summary>
        /// Gets the API scopes, which define the resources the API can access.
        /// </summary>
        public static IEnumerable<ApiScope> ApiScopes =>
            new List<ApiScope>
            {
                new ApiScope("api1", "My API") // Example API scope
            };

        /// <summary>
        /// Gets the clients, which represent applications that can access the IdentityServer.
        /// </summary>
        public static IEnumerable<Client> Clients =>
            new List<Client>
            {
                new Client
                {
                    ClientId = "client",
                    AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,
                    ClientSecrets = { new Secret("secret".Sha256()) },
                    AllowedScopes = { "openid", "profile", "api1", "roles" }, // Include "roles"
                    AllowOfflineAccess = true
                }
            };
    }
}
