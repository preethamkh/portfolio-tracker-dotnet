namespace PortfolioTracker.Core.Configuration;

/// <summary>
/// Configuration settings for Alpha Vantage API integration.
/// Alpha Vantage provides free stock market data.
/// </summary>
/// <remarks>
/// Alpha Vantage Free Tier Limits:
/// - 25 API calls per day
/// - 5 API calls per minute
/// 
/// I use Alpha Vantage to:
/// 1. Validate stock symbols (verify they exist)
/// 2. Fetch company information (name, sector, industry)
/// 3. Get current stock prices (for portfolio valuation)
/// 4. Fetch historical prices (for charts)
/// 
/// Caching Strategy:
/// - Security info: Cache forever (rarely changes)
/// - Current prices: Cache 15 minutes (balance freshness vs API limits)
/// - Historical prices: Cache 24 hours (static once market closes)
/// todo: would require looking at these settings / limits
/// </remarks>
public class AlphaVantageSettings
{
    /// <summary>
    /// Alpha Vantage API key.
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Base URL for Alpha Vantage API.
    /// Default: https://www.alphavantage.co/query
    /// </summary>
    public string BaseUrl { get; set; } = "https://www.alphavantage.co/query";

    /// <summary>
    /// Timeout for API requests (seconds).
    /// Alpha Vantage can be slow, especially for historical data.
    /// </summary>
    public int TimeoutInSeconds { get; set; } = 30;

    /// <summary>
    /// Enable caching of API responses to avoid rate limits.
    /// Highly recommended for free tier!
    /// </summary>
    public bool EnableCaching { get; set; } = true;

    /// <summary>
    /// Cache duration for security information (minutes).
    /// Security info rarely changes, so cache for long periods.
    /// </summary>
    public int SecurityInfoCacheMinutes { get; set; } = 259200; // 180 days

    /// <summary>
    /// Cache duration for current prices (minutes).
    /// Balance between freshness and API limits.
    /// </summary>
    public int PriceCacheMinutes { get; set; } = 15;
}
