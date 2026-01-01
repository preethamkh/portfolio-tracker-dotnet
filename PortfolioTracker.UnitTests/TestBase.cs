using Microsoft.Extensions.Logging;
using Moq;

namespace PortfolioTracker.UnitTests;

/// <summary>
/// Base class for test methods.
/// Provides common setup and helper methods to reduce boilerplate in individual test classes.
/// </summary>
public abstract class TestBase
{
    /// <summary>
    /// Creates a mock ILogger for any type.
    /// Loggers are required by services, but we don't care about them in tests.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    protected static ILogger<T> CreateMockLogger<T>()
    {
        return new Mock<ILogger<T>>().Object;
    }

    /// <summary>
    /// Create a Mock ILogger for any type with Verify capabilities.
    /// Use this when we want to verify that certain log messages were logged.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    protected static Mock<ILogger<T>> CreateMockLoggerWithVerify<T>()
    {
        return new Mock<ILogger<T>>();
    }
}