using PortfolioTracker.Core.DTOs.ExternalData;

namespace PortfolioTracker.Core.Interfaces.Services;

/// <summary>
/// Defines the contract for retrieving stock market data from external providers.
/// This abstraction allows switching between providers (Alpha Vantage, Finnhub, etc.)
/// without changing business logic.
/// </summary>
/// <remarks>
/// Why this interface exists:
/// - Decouples business logic from specific API providers
/// - Enables easy provider switching (when rate limits hit or better pricing found)
/// - Facilitates testing with mock implementations
/// - Allows caching layer to wrap any provider transparently
/// </remarks>
public interface IStockDataService
{
    /// <summary>
    /// Retrieves the current/latest quote for a security.
    /// Used for: Portfolio valuation, price displays, real-time updates
    /// </summary>
    Task<StockQuoteDto?> GetQuoteAsync(string symbol);

    /// <summary>
    /// Retrieves detailed company/fund information for a security.
    /// Used for: Adding new securities, displaying company details, sector analysis
    /// </summary>
    /// <remarks>
    /// For ETFs, sector/industry will be null.
    /// </remarks>
    Task<CompanyInfoDto?> GetCompanyInfoAsync(string symbol);

    /// <summary>
    /// Searches for securities by symbol or company name.
    /// Used for: Autocomplete search when user adds securities to portfolio
    /// </summary>
    /// <param name="query">Search term (partial symbol or company name)</param>
    /// <param name="limit">Maximum results to return (default: 10)</param>
    /// <returns>List of matching securities from the external API</returns>
    Task<List<ExternalSecuritySearchDto>> SearchSecuritiesAsync(string query, int limit = 10);

    /// <summary>
    /// Retrieves historical daily prices for charting and performance calculations.
    /// Used for: Portfolio performance charts, calculating returns over time
    /// </summary>
    /// <param name="symbol">Security symbol</param>
    /// <param name="startDate">Start date (inclusive)</param>
    /// <param name="endDate">End date (inclusive)</param>
    /// <returns>List of daily prices or null if error</returns>
    /// <remarks>
    /// This can be expensive (large data), so consider caching heavily.
    /// Some APIs charge per call for historical data.
    /// </remarks>
    Task<List<HistoricalPriceDto>?> GetHistoricalPricesAsync(string symbol, DateTime startDate, DateTime endDate);
}
