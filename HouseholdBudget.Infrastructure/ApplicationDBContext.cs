using Microsoft.EntityFrameworkCore;
using HouseholdBudget.Domain.Entities;

namespace HouseholdBudget.Infrastructure
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Define DbSets for your entities
        public DbSet<Expense> Expenses { get; set; }

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            configurationBuilder.Properties<decimal>()
                .HavePrecision(20, 2);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Fluent API configurations (optional)
            modelBuilder.Entity<Expense>(entity =>
            {
                entity.Property(e => e.Amount);
            });
        }
    }
}
