using Microsoft.EntityFrameworkCore;
using HouseholdBudget.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace HouseholdBudget.Infrastructure
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Define DbSets for your entities
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Account> Accounts { get; set; }

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
            modelBuilder.Entity<IdentityRole>().HasData(
                new IdentityRole { Id = "1", Name = "Admin", NormalizedName = "ADMIN" },
                new IdentityRole { Id = "2", Name = "User", NormalizedName = "USER" }
            );

            // Seed default admin user
            var adminUser = new IdentityUser
            {
                Id = "1",
                UserName = "admin@example.com",
                NormalizedUserName = "ADMIN@EXAMPLE.COM",
                Email = "admin@example.com",
                NormalizedEmail = "ADMIN@EXAMPLE.COM",
                EmailConfirmed = true,
                PasswordHash = "AQAAAAIAAYagAAAAEH0MHgXb85VobOE8N045dkFU/SISHU9gRMbIoqXMXHQpcH/JNYmnrfl3BFSo16xxew==",
                ConcurrencyStamp = "b1fd63fe-c93a-41b8-b96e-b6cf5d4fcc46",
                SecurityStamp = "1f1f3b55-9e3a-4f51-8f42-d0bb0a622562"
            };

            modelBuilder.Entity<IdentityUser>().HasData(adminUser);

            // Associate admin user with Admin role
            modelBuilder.Entity<IdentityUserRole<string>>().HasData(
                new IdentityUserRole<string> { UserId = "1", RoleId = "1" } // Admin User -> Admin Role
            );
        }
    }
}
