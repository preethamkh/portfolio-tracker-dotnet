using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using PortfolioTracker.Core.Interfaces.Services;
using PortfolioTracker.Infrastructure.Data;
using PortfolioTracker.IntegrationTests.Helpers;

namespace PortfolioTracker.IntegrationTests.Fixtures;

/// <summary>
/// Custom WebApplicationFactory that configures the test server.
/// This replaces the production database with an in-memory database for integration testing.
/// </summary>
/// <remarks>
/// WebApplicationFactory is the standard way in ASP.NET Core to create a test server for integration tests.
/// What it does:
/// 1. Creates an in-memory test server (no real HTTP server needed)
/// 2. Hosts your actual API application
/// 3. Allows you to make HTTP requests via HttpClient
/// 4. Lets you override services (like swapping real DB for test DB)
///
/// How it works:
/// - Inherits from WebApplicationFactory<Program/> where Program is your API's Program class.
/// - Overrides the ConfigureWebHost method to configure the test server / customize services.
/// - Tests create HttpClient instances from this factory to call API endpoints.
/// - Requests go through the full ASP.NET Core pipeline (middleware, routing, controllers).
/// i.e., in a controlled, test-friendly environment.
///
/// Why "in-memory"?
/// - No network calls (much faster)
/// - No port conflicts
/// - Isolated per test run
/// - Can run multiple tests in parallel
/// </remarks>
public class IntegrationTestWebAppFactory : WebApplicationFactory<Program>
{
    // Unique DB name per factory instance
    // Ensures isolation between test classes
    private readonly string _dbName = $"TestDb_{Guid.NewGuid():N}";

    /// <summary>
    /// Override this method to configure services for testing.
    /// This is where we swap out the real(prod) database with an in-memory database.
    /// </summary>
    /// <param name="builder">The web host builder</param>
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Optional
        // This sets ASPNETCORE_ENVIRONMENT to "Testing"
        // Can use this in Program.cs to configure test-specific behavior
        builder.UseEnvironment("Testing");
        builder.UseContentRoot(AppContext.BaseDirectory);

        // Call the base method to ensure default configuration
        base.ConfigureWebHost(builder);

        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddJsonFile("appsettings.Testing.json", optional: false, reloadOnChange: true);
        });

        // Override services for testing
        builder.ConfigureServices(services =>
        {
            // Step 1: Remove the production database context registration
            // We need to find and remove the existing DbContext registration
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));

            if (descriptor != null)
            {
                // because we don't want tests hitting prod database
                services.Remove(descriptor);
            }

            // Step 2: Register test database (in-memory database)
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                // UseInMemoryDatabase creates a fake database in memory
                // Database name must be unique per test to avoid conflicts
                options.UseInMemoryDatabase(_dbName);

                // Why in-memory?
                // - Fast: No disk I/O
                // - Isolated: Each test can have its own database instance
                // - No cleanup needed (disposed after test)

                // Trade-offs:
                // - Not REAL PostgreSQL, so some behaviors may differ
                // - Good enough for most tests
                // - For critical tests, we'll use Testcontainers with real PostgreSQL
            });

            // Remove the real IStockDataService registration
            var descriptors = services.Where(d => d.ServiceType == typeof(IStockDataService)).ToList();
            foreach (var d in descriptors)
                services.Remove(d);
            //var stockDataServiceDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IStockDataService));
            //if (stockDataServiceDescriptor != null)
            //{
            //    services.Remove(stockDataServiceDescriptor);
            //}
            // Register a mock/fake IStockDataService for testing
            services.AddSingleton<IStockDataService, MockStockDataService>();

            // Step 3: Ensure the database is created for each test
            var serviceProvider = services.BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // Create the database schema
            // This creates all table based on our DbContext model (entities)
            // Similar to running migrations, but for in-memory DB
            dbContext.Database.EnsureCreated();
        });
    }
}