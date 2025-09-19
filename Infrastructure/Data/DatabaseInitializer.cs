// Infrastructure/Data/DatabaseInitializer.cs
using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Data
{
    public static class DatabaseInitializer
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<AppDbContext>>();

            try
            {
                // Ensure database is created and up to date
                await context.Database.MigrateAsync();
                logger.LogInformation("Database migration completed successfully.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while migrating the database.");
                throw;
            }
        }

        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<AppDbContext>>();

            try
            {
                await SeedDefaultDataAsync(context, logger);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while seeding the database.");
                throw;
            }
        }

        private static async Task SeedDefaultDataAsync(AppDbContext context, ILogger logger)
        {
            // Check if data already exists
            if (await context.Users.AnyAsync())
            {
                logger.LogInformation("Database already contains data. Skipping seed.");
                return;
            }

            // Add additional seed data if needed
            logger.LogInformation("Seeding database with default data...");

            // The seed data is already defined in OnModelCreating method in AppDbContext
            // But you can add additional data here if needed

            await context.SaveChangesAsync();
            logger.LogInformation("Database seeding completed successfully.");
        }
    }

    public static class HostExtensions
    {
        public static async Task<IHost> MigrateDatabaseAsync(this IHost host)
        {
            using (var scope = host.Services.CreateScope())
            {
                await DatabaseInitializer.InitializeAsync(scope.ServiceProvider);
                await DatabaseInitializer.SeedAsync(scope.ServiceProvider);
            }
            return host;
        }
    }
}