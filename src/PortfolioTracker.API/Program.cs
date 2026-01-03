using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PortfolioTracker.Core.Configuration;
using PortfolioTracker.Core.Interfaces.Repositories;
using PortfolioTracker.Core.Interfaces.Services;
using PortfolioTracker.Core.Services;
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

// Register Services (Business Logic Layer)
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IPortfolioService, PortfolioService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();

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

        //http://localhost:3000: This is the default port for Create React App, a popular tool for bootstrapping React projects.
        //http://localhost:5173: This is the default port for Vite, a fast build tool often used with React and other front-end frameworks.

        policy.WithOrigins("http://localhost:3000", "http://localhost:5173") // React dev servers
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// Stock Data Service Configuration
// 1. Configure Alpha Vantage settings from appsettings.json
builder.Services.Configure<AlphaVantageSettings>(builder.Configuration.GetSection("AlphaVantage"));

// 2. Register HttpClient for AlphaVantageService
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
});
// Optional: Add retry policy using Polly (install Microsoft.Extensions.Http.Polly)
// .AddTransientHttpErrorPolicy(policy => 
//     policy.WaitAndRetryAsync(3, retryAttempt => 
//         TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))));

// IMPORTANT: When we add Redis caching(later), we'll wrap this registration
// The pattern will be:
// AlphaVantageService (makes API calls) 
//   -> wrapped by -> 
// StockDataCachingService (adds caching)
//   -> registered as -> 
// IStockDataService (what controllers use)


// EXPLANATION OF THE DI REGISTRATION

// What happens when something requests IStockDataService?
// 
// 1. ASP.NET DI container sees: "IStockDataService is needed"
// 2. Looks up registration: IStockDataService -> AlphaVantageService
// 3. Creates AlphaVantageService instance with:
//    - HttpClient (from AddHttpClient - pooled and configured)
//    - IOptions<AlphaVantageSettings> (from Configure() - bound to appsettings)
//    - ILogger<AlphaVantageService> (from AddLogging - already registered)
// 4. Returns the instance
// 
// Lifetime: Transient (new instance per request)
// - HttpClient is pooled underneath, so this is efficient
// - Each request gets a fresh service instance
// - No shared state between requests (thread-safe)


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
// Your business logic (SecurityService, etc.) never changes - only this registration!

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
            title:""
            );
    }
});

app.Run();

public partial class Program;