using Microsoft.EntityFrameworkCore;
using PortfolioTracker.Core.Interfaces.Repositories;
using PortfolioTracker.Core.Interfaces.Services;
using PortfolioTracker.Core.Services;
using PortfolioTracker.Infrastructure.Repositories;
using ApplicationDbContext = PortfolioTracker.Infrastructure.Data.ApplicationDbContext;
using DateTime = System.DateTime;
using Exception = System.Exception;
using Results = Microsoft.AspNetCore.Http.Results;

var builder = WebApplication.CreateBuilder(args);

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


// DI order does not matter as long as all dependencies are registered before they are used.

// Register Repositories (Data Access Layer)
// AddScoped: new instance is created per HTTP request. This is a common choice for data access services.
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IUserRepository, UserRepository>();

// Register Services (Business Logic Layer)
builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo()
    {
        Title = "Portfolio Tracker API",
        Version = "v1",
        Description = "API for tracking investment portfolios, tracking stocks and analyzing performance",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact()
        {
            Name = "Preetham K H",
            Email = "x@gmail.com",
            Url = new Uri("https://github.com/preethamkh/porfolio-tracker-dotnet")
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

// Enable authorization (todo: add authentication later)
app.UseAuthorization();

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
