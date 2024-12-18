using Microsoft.EntityFrameworkCore;
using HouseholdBudget.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using HouseholdBudget.Infrastructure.Interfaces;

namespace HouseholdBudget.Infrastructure
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        private readonly IEntitySaveInterceptor _entitySaveInterceptor;

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options,
                IEntitySaveInterceptor entitySaveInterceptor)
            : base(options)
        {
            _entitySaveInterceptor = entitySaveInterceptor;
        }

        // Define DbSets for your entities
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Account> Accounts { get; set; }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            _entitySaveInterceptor.InterceptSave(ChangeTracker);
            return await base.SaveChangesAsync(cancellationToken);
        }

        public override int SaveChanges()
        {
            _entitySaveInterceptor.InterceptSave(ChangeTracker);
            return base.SaveChanges();
        }

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            configurationBuilder.Properties<decimal>()
                .HavePrecision(20, 2);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Fluent API configurations (optional)
            modelBuilder.Entity<Account>();
            modelBuilder.Entity<Transaction>();

            // Seed roles
            modelBuilder.Entity<IdentityRole>().HasData(Domain.Roles.Predefined.GetAll());

            string userId = "9E695B49-3EEA-4BAC-9A0F-73E21977E65F";
            string defaultAdminEmail = "admin@householdbudget.com";

            // Seed default admin user
            var adminUser = new IdentityUser
            {
                Id = userId,
                UserName = defaultAdminEmail,
                NormalizedUserName = defaultAdminEmail.ToUpper(),
                Email = defaultAdminEmail,
                NormalizedEmail = defaultAdminEmail.ToUpper(),
                EmailConfirmed = true,
                PasswordHash = "AQAAAAIAAYagAAAAEH0MHgXb85VobOE8N045dkFU/SISHU9gRMbIoqXMXHQpcH/JNYmnrfl3BFSo16xxew==",
                ConcurrencyStamp = "b1fd63fe-c93a-41b8-b96e-b6cf5d4fcc46",
                SecurityStamp = "1f1f3b55-9e3a-4f51-8f42-d0bb0a622562"
            };

            modelBuilder.Entity<IdentityUser>().HasData(adminUser);

            // Associate admin user with Admin role
            modelBuilder.Entity<IdentityUserRole<string>>().HasData(
                new IdentityUserRole<string> { UserId = userId, RoleId = Domain.Roles.Definitions.Admin.Id.ToString() } // Admin User -> Admin Role
            );
        }
    }
}
