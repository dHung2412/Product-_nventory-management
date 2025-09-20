// Program.cs - Updated seeding section
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Infrastructure;
using Infrastructure.Data;
using Application.Interfaces;
using Application.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Domain.Entities;
using Infrastructure.Security;

var builder = WebApplication.CreateBuilder(args);

// Configure URLs to avoid port conflicts
builder.WebHost.UseUrls("http://localhost:5080", "https://localhost:5443");

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Configure Swagger/OpenAPI
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Warehouse Management API",
        Version = "v1",
        Description = "A comprehensive warehouse management system API",
        Contact = new OpenApiContact
        {
            Name = "Warehouse Management Team",
            Email = "support@warehouse.com"
        }
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
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

    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

// Configure CORS
var corsSettings = builder.Configuration.GetSection("CorsSettings");
var allowedOrigins = corsSettings.GetSection("AllowedOrigins").Get<string[]>() ?? new[] { "*" };
var allowCredentials = corsSettings.GetValue<bool>("AllowCredentials");

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins", policy =>
    {
        if (allowedOrigins.Contains("*"))
        {
            policy.AllowAnyOrigin();
        }
        else
        {
            policy.WithOrigins(allowedOrigins);
        }
        
        policy.AllowAnyMethod()
              .AllowAnyHeader();
              
        if (allowCredentials)
        {
            policy.AllowCredentials();
        }
    });
});

// Configure JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"] ?? "your-super-secret-key-that-is-at-least-32-characters-long";
var issuer = jwtSettings["Issuer"] ?? "WarehouseManagement";
var audience = jwtSettings["Audience"] ?? "WarehouseManagementClient";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = issuer,
            ValidAudience = audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
            ClockSkew = TimeSpan.Zero
        };

        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                {
                    context.Response.Headers.Append("Token-Expired", "true");
                }
                return Task.CompletedTask;
            }
        };
    });

// Configure Authorization
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("ManagerOrAdmin", policy => policy.RequireRole("Admin", "Manager"));
    options.AddPolicy("EmployeeOrAbove", policy => policy.RequireRole("Admin", "Manager", "Employee"));
});

// Add Infrastructure Services
builder.Services.AddInfrastructure(builder.Configuration);

// Add Application Services
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IWarehouseService, WarehouseService>();
builder.Services.AddScoped<IStockService, StockService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IJwtService, JwtService>();

// Register the custom password hasher
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher>();

// Configure Health Checks
var healthChecksConfig = builder.Configuration.GetSection("HealthChecks");
if (healthChecksConfig.GetValue<bool>("Enabled"))
{
    builder.Services.AddHealthChecks()
        .AddDbContextCheck<AppDbContext>("database", tags: new[] { "ready" });
}

// Configure Logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// Configure API Behavior
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.SuppressModelStateInvalidFilter = true;
});

// Add Response Compression
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Warehouse Management API v1");
        c.RoutePrefix = string.Empty;
        c.DisplayRequestDuration();
        c.EnableDeepLinking();
        c.EnableFilter();
        c.ShowExtensions();
        c.EnableValidator();
    });
}

app.UseResponseCompression();
app.UseCors("AllowSpecificOrigins");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Map health checks
if (healthChecksConfig.GetValue<bool>("Enabled"))
{
    app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
    {
        ResponseWriter = async (context, report) =>
        {
            context.Response.ContentType = "application/json";
            var result = System.Text.Json.JsonSerializer.Serialize(new
            {
                status = report.Status.ToString(),
                checks = report.Entries.Select(entry => new
                {
                    name = entry.Key,
                    status = entry.Value.Status.ToString(),
                    exception = entry.Value.Exception?.Message,
                    duration = entry.Value.Duration.ToString()
                })
            });
            await context.Response.WriteAsync(result);
        }
    });

    app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
    {
        Predicate = check => check.Tags.Contains("ready")
    });

    app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
    {
        Predicate = _ => false
    });
}

// Database initialization and seeding
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<User>>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    
    try
    {
        // Xóa EnsureCreatedAsync() - chỉ sử dụng MigrateAsync()
        if (context.Database.GetPendingMigrations().Any())
        {
            await context.Database.MigrateAsync();
            logger.LogInformation("Database migrations applied successfully");
        }
        else
        {
            await context.Database.MigrateAsync();  // Vẫn gọi để đảm bảo cơ sở dữ liệu tồn tại và schema đúng
            logger.LogInformation("No pending migrations, but database ensured via Migrate");
        }
        
        await SeedInitialDataAsync(context, passwordHasher, logger);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while initializing the database");
        throw;
    }
}

// Global exception handling
app.UseExceptionHandler("/error");

app.Map("/error", (HttpContext context) =>
{
    var exception = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>()?.Error;
    var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
    logger.LogError(exception, "Unhandled exception occurred");
    
    return Results.Problem(
    detail: app.Environment.IsDevelopment() ? exception?.Message : "An error occurred",
    statusCode: 500
);
});

app.MapGet("/", () => Results.Redirect("/swagger"))
    .WithName("Welcome")
    .WithTags("General");

app.MapGet("/api/info", () => new
{
    Name = "Warehouse Management API",
    Version = "v1.0.0",
    Description = "A comprehensive warehouse management system",
    Environment = app.Environment.EnvironmentName,
    Timestamp = DateTime.UtcNow
})
.WithName("ApiInfo")
.WithTags("General");

app.Logger.LogInformation("🚀 Warehouse Management API is starting up...");
var swaggerHttpsUrl = app.Urls.FirstOrDefault(u => u.StartsWith("https://")) ?? "https://localhost:5443";
app.Logger.LogInformation("📊 Swagger UI available at: {SwaggerUrl}", app.Environment.IsDevelopment() ? swaggerHttpsUrl : "N/A");
app.Logger.LogInformation("🏥 Health checks available at: /health");

app.Run();

// FIXED: Helper method for seeding initial data with correct password hashing
static async Task SeedInitialDataAsync(AppDbContext context, IPasswordHasher<User> passwordHasher, ILogger logger)
{
    try
    {
        if (await context.Users.AnyAsync())
        {
            logger.LogInformation("Database already contains data, skipping seed");
            return;
        }

        // Create default admin user with correct password hashing
        var adminUser = new User("admin", "admin@warehouse.com", "", Role.Admin, true);
        var hashedPassword = passwordHasher.HashPassword(adminUser, "admin123");
        
        // Update the user with the hashed password
        adminUser.Update("admin", "admin@warehouse.com", hashedPassword, Role.Admin, true);
        
        context.Users.Add(adminUser);

        // Create additional test users
        var managerUser = new User("manager", "manager@warehouse.com", "", Role.Manager, true);
        var managerHashedPassword = passwordHasher.HashPassword(managerUser, "manager123");
        managerUser.Update("manager", "manager@warehouse.com", managerHashedPassword, Role.Manager, true);
        
        var employeeUser = new User("employee", "employee@warehouse.com", "", Role.Employee, true);
        var employeeHashedPassword = passwordHasher.HashPassword(employeeUser, "employee123");
        employeeUser.Update("employee", "employee@warehouse.com", employeeHashedPassword, Role.Employee, true);
        
        context.Users.AddRange(managerUser, employeeUser);

        // Create sample products
        var products = new[]
        {
            new Domain.Entities.Product("Laptop", "High-performance laptop", "pcs", 999.99m, "Electronics"),
            new Domain.Entities.Product("T-Shirt", "Cotton t-shirt", "pcs", 19.99m, "Clothing"),
            new Domain.Entities.Product("Smartphone", "Latest smartphone model", "pcs", 699.99m, "Electronics"),
            new Domain.Entities.Product("Jeans", "Denim jeans", "pcs", 49.99m, "Clothing"),
            new Domain.Entities.Product("Book", "Programming book", "pcs", 29.99m, "Books")
        };

        context.Products.AddRange(products);

        // Create sample warehouses
        var warehouses = new[]
        {
            new Domain.Entities.Warehouse("Main Warehouse", "123 Main St, City, State"),
            new Domain.Entities.Warehouse("Secondary Warehouse", "456 Second St, City, State"),
            new Domain.Entities.Warehouse("Storage Facility", "789 Storage Ave, City, State")
        };

        context.Warehouses.AddRange(warehouses);

        await context.SaveChangesAsync();

        logger.LogInformation("✅ Initial data seeded successfully");
        logger.LogInformation("👤 Default users created:");
        logger.LogInformation("   🔑 Admin: admin / admin123");
        logger.LogInformation("   👨‍💼 Manager: manager / manager123");
        logger.LogInformation("   👨‍💻 Employee: employee / employee123");
        logger.LogInformation("📦 {ProductCount} sample products created", products.Length);
        logger.LogInformation("🏢 {WarehouseCount} sample warehouses created", warehouses.Length);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "❌ Error seeding initial data");
        throw;
    }
}