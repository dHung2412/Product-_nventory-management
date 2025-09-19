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
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Configure URLs to avoid port conflicts (change ports here as needed)
builder.WebHost.UseUrls("http://localhost:5080", "https://localhost:5443");

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
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

    // Add JWT Authentication to Swagger
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

    // Include XML comments
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
var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");
var issuer = jwtSettings["Issuer"] ?? throw new InvalidOperationException("JWT Issuer not configured");
var audience = jwtSettings["Audience"] ?? throw new InvalidOperationException("JWT Audience not configured");

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
        c.RoutePrefix = string.Empty; // Set Swagger UI at the app's root
        c.DisplayRequestDuration();
        c.EnableDeepLinking();
        c.EnableFilter();
        c.ShowExtensions();
        c.EnableValidator();
    });
}

// Enable response compression
app.UseResponseCompression();

// Enable CORS
app.UseCors("AllowSpecificOrigins");

// Enable HTTPS redirection
app.UseHttpsRedirection();

// Enable Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// Map controllers
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
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    
    try
    {
        // Ensure database is created
        await context.Database.EnsureCreatedAsync();
        logger.LogInformation("Database created successfully");
        
        // Run migrations if any
        if (context.Database.GetPendingMigrations().Any())
        {
            await context.Database.MigrateAsync();
            logger.LogInformation("Database migrations applied successfully");
        }
        
        // Seed initial data
        await SeedInitialDataAsync(context, logger);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while initializing the database");
        throw;
    }
}

// Global exception handling
app.UseExceptionHandler("/error");

// Add a custom error endpoint
app.Map("/error", (HttpContext context) =>
{
    var exception = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>()?.Error;
    var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
    logger.LogError(exception, "Unhandled exception occurred");
    
    return Results.Problem(
        title: "An error occurred",
        detail: app.Environment.IsDevelopment() ? exception?.Message : "An error occurred",
        statusCode: 500
    );
});

// Add a welcome endpoint
app.MapGet("/", () => Results.Redirect("/swagger"))
    .WithName("Welcome")
    .WithTags("General");

// Add API info endpoint
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

// Helper method for seeding initial data
static async Task SeedInitialDataAsync(AppDbContext context, ILogger logger)
{
    try
    {
        // Check if data already exists
        if (await context.Users.AnyAsync())
        {
            logger.LogInformation("Database already contains data, skipping seed");
            return;
        }

        // Create default admin user
        var adminUser = new Domain.Entities.User(
            "admin", 
            "admin@warehouse.com", 
            BCrypt.Net.BCrypt.HashPassword("admin123"), 
            Domain.Entities.Role.Admin, 
            true
        );

        context.Users.Add(adminUser);

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
        logger.LogInformation("👤 Default admin user created: admin / admin123");
        logger.LogInformation("📦 {ProductCount} sample products created", products.Length);
        logger.LogInformation("🏢 {WarehouseCount} sample warehouses created", warehouses.Length);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "❌ Error seeding initial data");
        throw;
    }
}
