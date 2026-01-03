using PortfolioTracker.Core.DTOs.ExternalData;
using PortfolioTracker.Core.DTOs.Security;

namespace PortfolioTracker.Core.Interfaces.Services;

public interface IStockDataService
{
    /// <summary>
    /// Get current price for a stock symbol
    /// </summary>
    Task<StockQuoteDto?> GetQuoteAsync(string symbol);

    /// <summary>
    /// Get detailed company information
    /// </summary>
    Task<CompanyInfoDto?> GetCompanyInfoAsync(string symbol);

    /// <summary>
    /// Search for securities by symbol or name
    /// </summary>
    Task<List<SecuritySearchDto>> SearchSecuritiesAsync(string query, int limit = 10);

    /// <summary>
    /// Get historical price data
    /// </summary>
    Task<List<HistoricalPriceDto?>> GetHistoricalPricesAsync(string symbol, DateTime startDate, DateTime endDate);
}
