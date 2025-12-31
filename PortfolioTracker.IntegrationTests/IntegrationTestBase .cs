using Microsoft.Extensions.DependencyInjection;
using PortfolioTracker.Infrastructure.Data;
using PortfolioTracker.IntegrationTests.Fixtures;

namespace PortfolioTracker.IntegrationTests;

/// <summary>
/// Base class for all integration tests.
/// Provides common setup and utilities for integration testing.
/// </summary>
/// <remarks>
/// Why use IClassFixture?
/// - xUnit creates one instance of the fixture for ALL tests in the class
/// - The factory (with the test server) is reused across tests == faster tests
/// - Database is created once per test class, not per test
/// - Trade-off: Tests in the same class share fixture (but get clean DB each time)
///
/// Lifecycle:
/// 1. xUnit creates IntegrationTestWebAppFactory (once per test class)
/// 2. For each test:
///    - Constructor runs
///    - Test runs
///    - Dispose runs (cleanup)
/// 3. After all tests: Factory disposed
/// </remarks>
public abstract class IntegrationTestBase : IClassFixture<IntegrationTestWebAppFactory>, IDisposable
{
    /// <summary>
    /// HTTP client for making requests to the test server.
    /// Pre-configured to talk to in-memory test server.
    /// </summary>
    protected readonly HttpClient Client;

    /// <summary>
    /// The test factory (provies access to services).
    /// </summary>
    protected readonly IntegrationTestWebAppFactory Factory;

    /// <summary>
    /// Database context for direct database access during tests.
    /// Use this to verify data was saved correctly.
    /// </summary>
    protected readonly ApplicationDbContext Context;

    /// <summary>
    /// Service scope (needed for scoped services like DbContext).
    /// </summary>
    private readonly IServiceScope _scope;

    /// <summary>
    /// Constructor runs BEFORE EACH test.
    /// Sets up clean state for each test.
    /// </summary>
    /// <param name="factory">The test factory (injected by xUnit)</param>
    protected IntegrationTestBase(IntegrationTestWebAppFactory factory)
    {
        Factory = factory;

        // Create HTTP client (talks to in-memory test server)
        Client = factory.CreateClient();

        // Get database context from DI
        // Why scope? DbContext is scoped (one per request)
        // factory.Services in the test in a reference to the DI container built from the registrations in Program.cs in the main project.
        _scope = factory.Services.CreateScope();
        // asking for an instance of a service of type ApplicationDbContext from the service provider associated with the created scope. (from Program.cs registrations)
        Context = _scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Clean DB before each test
        CleanDatabase();
    }

    /// <summary>
    /// Cleans all data from database.
    /// Ensures each test starts with empty database (test isolation).
    /// </summary>
    /// <remarks>
    /// Why clean instead of recreate?
    /// - Faster (don't drop/recreate schema)
    /// - Schema already exists from EnsureCreated()
    /// - Just remove data, keep structure
    /// </remarks>
    private void CleanDatabase()
    {
        // Remove all data (order matters due to foreign keys)
        Context.Portfolios.RemoveRange(Context.Portfolios);
        Context.Users.RemoveRange(Context.Users);
        // todo: Currently have this in TestDataBuilder
        Context.Securities.RemoveRange(Context.Securities);
        Context.SaveChanges();
    }


    // todo: question how does it get clean db each time? and why abstract class?
    public void Dispose()
    {
        // we don't dispose Client or Factory as they are reused across tests
        // xUnit manages their lifetimes (Factory by xUnit, Client by Factory)
        _scope.Dispose();
    }
}