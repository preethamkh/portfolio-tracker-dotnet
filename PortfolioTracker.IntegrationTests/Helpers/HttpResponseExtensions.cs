namespace PortfolioTracker.IntegrationTests.Helpers;

/// <summary>
/// Extension methods for HttpResponse to simplify test assertions.
/// </summary>
public static class HttpResponseExtensions
{
    /// <summary>
    /// Checks if the HTTP response indicates a successful status code (2xx).
    /// </summary>
    /// <remarks>
    /// Use with FluentAssertions like:
    /// response.IsSuccessful().Should().BeTrue();
    ///
    /// Or:
    /// response.Should().BeSuccessful(); // FluentAssertions has this built-in
    /// </remarks>
    public static bool IsSuccessful(this HttpResponseMessage response)
    {
        return response.IsSuccessStatusCode;
    }

    /// <summary>
    /// Gets response status code as integer.
    /// </summary>
    public static int GetStatusCodeAsInt(this HttpResponseMessage response)
    {
        return (int)response.StatusCode;
    }
}