namespace PortfolioTracker.Infrastructure.Configuration;

/// <summary>
/// Settings for caching stock data fetched from external APIs.
/// </summary>
public class StockDataCacheSettings
{
    /// <summary>
    /// How long to cache stock quotes (prices).
    /// Balance: Fresher data vs fewer API calls
    /// </summary>
    /// <remarks>
    /// 15 minutes is reasonable for portfolio tracking:
    /// - Stock prices don't change every second for retail investors
    /// 
    /// For day trading app: Would use 1-5 minutes
    /// For end-of-day portfolio: Could use 60 minutes or longer
    /// </remarks>
    public int QuoteCacheDurationMinutes { get; set; } = 1440;

    /// <summary>
    /// How long to cache company information (name, sector, etc.).
    /// This data rarely changes, so long cache is appropriate.
    /// </summary>
    public int CompanyInfoCacheDurationDays { get; set; } = 180;

    /// <summary>
    /// How long to cache historical price data.
    /// Historical data doesn't change (past is fixed), but we cache shorter
    /// because users might want data refreshed when new days are added.
    /// </summary>
    public int HistoricalDataCacheDurationDays { get; set; } = 1;
}
