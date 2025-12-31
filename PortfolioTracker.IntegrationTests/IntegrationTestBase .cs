using Microsoft.Extensions.DependencyInjection;
using PortfolioTracker.Infrastructure.Data;
using PortfolioTracker.IntegrationTests.Fixtures;

namespace PortfolioTracker.IntegrationTests;

/// <summary>
/// Base class for all integration tests.
/// Provides common setup and utilities for integration testing.
/// </summary>
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

        _scope = factory.Services.CreateScope();
        Context = _scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Clean DB before each test
        CleanDatabase();
    }

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