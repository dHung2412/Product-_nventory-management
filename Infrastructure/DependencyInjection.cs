// Infrastructure/DependencyInjection.cs
using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Data;
using Infrastructure.Repositories;
using Infrastructure.Security;
using Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // Database Configuration
            services.AddDbContext<AppDbContext>(options =>
            {
                var connectionString = configuration.GetConnectionString("DefaultConnection");
                options.UseSqlServer(connectionString, b => 
                {
                    b.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName);
                    b.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorNumbersToAdd: null);
                });

                // Enable sensitive data logging in development
                if (configuration.GetValue<bool>("Logging:EnableSensitiveDataLogging"))
                {
                    options.EnableSensitiveDataLogging();
                }
            });

            // Repository Registration
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<IWarehouseRepository, WarehouseRepository>();
            services.AddScoped<IStockItemRepository, StockItemRepository>();
            services.AddScoped<IStockTransactionRepository, StockTransactionRepository>();
            services.AddScoped<IUserRepository, UserRepository>();

            // Security Services
            services.AddScoped<IPasswordHasher<User>, PasswordHasher>();

            // Infrastructure Services
            services.AddScoped<INotificationService, EmailNotificationService>();

            // Health Checks
            services.AddHealthChecks()
                .AddDbContextCheck<AppDbContext>();

            return services;
        }

        public static IServiceCollection AddInfrastructureWithInMemoryDatabase(this IServiceCollection services)
        {
            // For testing purposes - use in-memory database
            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase("WarehouseManagementTestDb"));

            // Repository Registration
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<IWarehouseRepository, WarehouseRepository>();
            services.AddScoped<IStockItemRepository, StockItemRepository>();
            services.AddScoped<IStockTransactionRepository, StockTransactionRepository>();
            services.AddScoped<IUserRepository, UserRepository>();

            // Security Services
            services.AddScoped<IPasswordHasher<User>, PasswordHasher>();

            return services;
        }
    }
}