using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Polly;
using PortfolioTracker.API.Data;
using PortfolioTracker.Core.Configuration;
using PortfolioTracker.Core.Interfaces.Repositories;
using PortfolioTracker.Core.Interfaces.Services;
using PortfolioTracker.Core.Services;
using PortfolioTracker.Infrastructure.Configuration;
using PortfolioTracker.Infrastructure.Repositories;
using PortfolioTracker.Infrastructure.Services;
using System.Text;
using ApplicationDbContext = PortfolioTracker.Infrastructure.Data.ApplicationDbContext;
using DateTime = System.DateTime;
using Exception = System.Exception;
using Results = Microsoft.AspNetCore.Http.Results;

var builder = WebApplication.CreateBuilder(args);

// Configure services and middleware
// Bind Jwt settings from configuration
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));

// Get JWT settings for authentication configuration
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>()
                  ?? throw new InvalidOperationException("JWT settings not configured");

// Add services to the container.

// Configure Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    // Get connection string from appsettings.json
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

    // Use PostgreSQL with Npgsql provider
    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
        // Enable retry on failure (resilience)
        npgsqlOptions.EnableRetryOnFailure(
            maxRetryCount: 3,
            maxRetryDelay: TimeSpan.FromSeconds(5),
            errorCodesToAdd: null);

        // Set command timeout
        npgsqlOptions.CommandTimeout(30);
    });

    // Enable detailed errors and sensitive data logging in Development environment
    if (builder.Environment.IsDevelopment())
    {
        // Shows parameter values in logs
        options.EnableSensitiveDataLogging();
        // Shows detailed error messages
        options.EnableDetailedErrors();
    }
});

// Authentication and Authorization
// Configure JWT Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret)),
        // todo: 5 mins or zero?
        ClockSkew = TimeSpan.Zero // No clock skew
    };

    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            //if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
            //{
            //    context.Response.Headers.Add("Token-Expired", "true");
            //}

            // Log authentication failures
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogError(context.Exception, "Authentication failed");
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            // Log successful token validation
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogInformation("Token validated for {User}", context.Principal?.Identity?.Name);
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();

// DI order does not matter as long as all dependencies are registered before they are used.

// Register Repositories (Data Access Layer)
// AddScoped: new instance is created per HTTP request. This is a common choice for data access services.
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IPortfolioRepository, PortfolioRepository>();
builder.Services.AddScoped<ISecurityRepository, SecurityRepository>();
builder.Services.AddScoped<IHoldingRepository, HoldingRepository>();
builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();

// Register Services (Business Logic Layer)
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IPortfolioService, PortfolioService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<ISecurityService, SecurityService>();
builder.Services.AddScoped<IHoldingService, HoldingService>();
builder.Services.AddScoped<ITransactionService, TransactionService>();

builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Portfolio Tracker API",
        Version = "v1",
        Description = "API for tracking investment portfolios, tracking stocks and analyzing performance",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "Preetham K H",
            Email = "x@gmail.com",
            Url = new Uri("https://github.com/preethamkh/porfolio-tracker-dotnet")
        }
    });

    // Add JWT authentication to Swagger UI
    // todo: add to other endpoints and controllers as well?
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Enter 'Bearer' followed by a space and your JWT token"
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Add CORS (Cross-Origin Resource Sharing) for React frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins(
                "http://localhost:3000",
                "https://portfoliotracker-react.vercel.app",
                // Allow all Vercel preview deployments
                "https://*.vercel.app")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// ======================================================
// REDIS CACHING CONFIGURATION
// ======================================================

// Configure Redis settings from appsettings.json
builder.Services.Configure<RedisSettings>(
    builder.Configuration.GetSection("Redis"));

// Configure cache duration settings
builder.Services.Configure<StockDataCacheSettings>(
    builder.Configuration.GetSection("StockDataCache"));

// Add Redis distributed cache (IDistributedCache implementation)
// (bonus - switch to different cache providers easily in future)
var redisSettings = builder.Configuration.GetSection("Redis").Get<RedisSettings>();
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = redisSettings?.ConnectionString ?? "localhost:6379";
    options.InstanceName = redisSettings?.InstanceName ?? "PortfolioTracker:";
});


// ======================================================
// STOCK DATA SERVICE WITH CACHING (DECORATOR PATTERN)
// ======================================================

// 1. Configure Alpha Vantage settings from appsettings.json
builder.Services.Configure<AlphaVantageSettings>(builder.Configuration.GetSection("AlphaVantage"));

// todo: This is for the TestController (remove if I decide to delete)
builder.Services.AddHttpClient<AlphaVantageService, AlphaVantageService>(client => {
    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.Add("User-Agent", "PortfolioTracker/1.0");
}).AddTransientHttpErrorPolicy(policy =>
    policy.WaitAndRetryAsync(3, retryAttempt =>
        TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))));

// 2. Register HttpClient for AlphaVantageService
// WHY AddHttpClient instead of new HttpClient()?
// - Prevents socket exhaustion (reuses HttpClient instances)
// - Adds resilience (can add Polly retry policies)
// - Easier to mock in tests
// - Configures default settings centrally
builder.Services.AddHttpClient<IStockDataService, AlphaVantageService>(client =>
{
    // Set reasonable timeout for API calls
    client.Timeout = TimeSpan.FromSeconds(30);

    // Alpha Vantage recommends User-Agent header
    client.DefaultRequestHeaders.Add("User-Agent", "PortfolioTracker/1.0");

    // Optional: Add retry policy using Polly (install Microsoft.Extensions.Http.Polly)
}).AddTransientHttpErrorPolicy(policy =>
     policy.WaitAndRetryAsync(3, retryAttempt =>
         TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))));

// NOTE: this overrides the previous IStockDataService registration and DECORATES it (registers a factory that wraps AlphaVantageService)
// Last registration wins in DI container
builder.Services.AddScoped<IStockDataService>(serviceProvider =>
{
    // Resolve the inner AlphaVantageService
    var alphaVantageService = serviceProvider.GetRequiredService<AlphaVantageService>();
    // Resolve other dependencies for caching
    var cache = serviceProvider.GetRequiredService<Microsoft.Extensions.Caching.Distributed.IDistributedCache>();
    var logger = serviceProvider.GetRequiredService<ILogger<StockDataCachingService>>();
    var cacheSettings = serviceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<StockDataCacheSettings>>();
    // Return the caching decorator wrapping the AlphaVantageService
    return new StockDataCachingService(
        alphaVantageService,
        cache,
        logger,
        cacheSettings);
});


// EXPLANATION OF THE DI REGISTRATION

// ======================================================
// What happens when something requests IStockDataService?
//
// 1. DI container runs the factory lambda above
// 2. Creates AlphaVantageService (with HttpClient, settings, logger)
// 3. Gets IDistributedCache (Redis connection)
// 4. Creates StockDataCachingService wrapping AlphaVantageService
// 5. Returns StockDataCachingService as IStockDataService
//
// Flow when GetQuoteAsync("AAPL") is called:
// Controller -> IStockDataService (StockDataCachingService)
//   - checks Redis cache
//   - if cache miss, calls AlphaVantageService
//   - AlphaVantageService calls Alpha Vantage API
//   - StockDataCachingService stores result in Redis
//   - returns to Controller
// ======================================================


// FUTURE: SWITCHING PROVIDERS


// When you want to switch from Alpha Vantage to Finnhub:
// 
// Option 1: Simple switch (replace registration)
// builder.Services.AddHttpClient<IStockDataService, FinnhubService>(...);
// 
// Option 2: Configuration-based switching
// var provider = builder.Configuration["StockDataProvider:ActiveProvider"];
// if (provider == "AlphaVantage")
//     builder.Services.AddHttpClient<IStockDataService, AlphaVantageService>(...);
// else if (provider == "Finnhub")
//     builder.Services.AddHttpClient<IStockDataService, FinnhubService>(...);
// 
// Option 3: Factory pattern (advanced - use both providers)
// builder.Services.AddHttpClient<AlphaVantageService>(...);
// builder.Services.AddHttpClient<FinnhubService>(...);
// builder.Services.AddSingleton<IStockDataServiceFactory, StockDataServiceFactory>();
// 
// Business logic (SecurityService, etc.) never changes - only this registration!

var app = builder.Build();

// Configure the HTTP request pipeline.

// Enable Swagger in Development environment
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Portfolio Tracker API V1");
        //options.RoutePrefix = string.Empty; // Set Swagger UI at app's root
    });
}

// Use HTTPS redirection
app.UseHttpsRedirection();

// Enable CORS
app.UseCors("AllowReactApp");

// Enable Authentication and Authorization
// Authentication must come before Authorization
app.UseAuthentication(); // validates JWT token
app.UseAuthorization(); // checks user permissions [Authorize]

// Map controller endpoints
app.MapControllers();

// Database health check endpoint
// API available on port 7001, call using http://localhost:7001/health
app.MapGet("/health", async (ApplicationDbContext dbContext) =>
{
    try
    {
        // Try to connect to the database
        await dbContext.Database.CanConnectAsync();
        return Results.Ok(new
        {
            status = "Healthy",
            timestamp = DateTime.UtcNow,
            database = "Connected"
        });
    }
    catch (Exception ex)
    {
        return Results.Problem(
            detail: ex.Message,
            statusCode: 503,
            title: ""
            );
    }
});

// =============================================================================
// SEED DATA (Development Only)
// =============================================================================

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();

    var services = scope.ServiceProvider;

    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();

        // Ensure database is created
        await context.Database.EnsureCreatedAsync();

        // Run seed data
        await SeedDataScript.SeedTestDataAsync(context);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

app.Run();

public partial class Program;