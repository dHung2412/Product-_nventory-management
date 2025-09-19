// Infrastructure/Data/AppDbContext.cs
using System;
using System.Reflection;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // DbSets for all entities
        public DbSet<Product> Products { get; set; }
        public DbSet<Warehouse> Warehouses { get; set; }
        public DbSet<StockItem> StockItems { get; set; }
        public DbSet<StockTransaction> StockTransactions { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Apply all configurations from the current assembly
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            // Configure enum conversions
            modelBuilder.Entity<StockTransaction>()
                .Property(e => e.Type)
                .HasConversion<string>();

            modelBuilder.Entity<User>()
                .Property(e => e.Role)
                .HasConversion<string>();

            // Add some seed data
            SeedData(modelBuilder);
        }

        private static void SeedData(ModelBuilder modelBuilder)
        {
            // Seed default admin user
            modelBuilder.Entity<User>().HasData(
                new User("admin", "admin@warehouse.com",
                    "$2a$11$rQw8Q8Q8Q8Q8Q8Q8Q8Q8Q8Q8Q8Q8Q8Q8Q8Q8Q8Q8Q8Q8Q8",
                    Role.Admin, true)
            );

            // Seed sample categories
            modelBuilder.Entity<Product>().HasData(
                new Product("Laptop", "High-performance laptop", "pcs", 999.99m, "Electronics"),
                new Product("T-Shirt", "Cotton t-shirt", "pcs", 19.99m, "Clothing")
            );

            // Seed sample warehouses
            modelBuilder.Entity<Warehouse>().HasData(
                new Warehouse("Main Warehouse", "123 Main St, City, State"),
                new Warehouse("Secondary Warehouse", "456 Second St, City, State")
            );
        }

        public override int SaveChanges()
        {
            UpdateTimestamps();
            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateTimestamps();
            return await base.SaveChangesAsync(cancellationToken);
        }

        private void UpdateTimestamps()
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

            foreach (var entry in entries)
            {
                // If you add CreatedAt/UpdatedAt properties to your entities later
                if (entry.Entity is IAuditable auditableEntity)
                {
                    if (entry.State == EntityState.Added)
                    {
                        auditableEntity.CreatedAt = DateTime.UtcNow;
                    }
                    auditableEntity.UpdatedAt = DateTime.UtcNow;
                }
            }
        }
    }

    // Interface for auditable entities (optional)
    public interface IAuditable
    {
        DateTime CreatedAt { get; set; }
        DateTime UpdatedAt { get; set; }
    }
}