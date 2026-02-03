using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PortfolioTracker.Core.DTOs.ExternalData;
using PortfolioTracker.Core.Helpers;
using PortfolioTracker.Core.Interfaces.Services;
using PortfolioTracker.Infrastructure.Configuration;
using System.Text.Json;

namespace PortfolioTracker.Infrastructure.Services;

/// <summary>
/// Yahoo Finance API implementation via RapidAPI.
/// Provides stock quotes, company info, search, and historical data.
/// </summary>
public class YahooFinanceService : IStockDataService
{
    private readonly HttpClient _httpClient;
    private readonly YahooFinanceSettings _settings;
    private readonly ILogger<YahooFinanceService> _logger;

    public YahooFinanceService(
        HttpClient httpClient,
        IOptions<YahooFinanceSettings> settings,
        ILogger<YahooFinanceService> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _logger = logger;

        // Configure RapidAPI headers (required for authentication)
        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("X-RapidAPI-Key", _settings.ApiKey);
        _httpClient.DefaultRequestHeaders.Add("X-RapidAPI-Host", _settings.RapidApiHost);
    }

    public async Task<StockQuoteDto?> GetQuoteAsync(string symbol)
    {
        try
        {
            var url = $"{_settings.BaseUrl}/api/stock/get-price?region={_settings.Region}&symbol={symbol}";

            _logger.LogInformation("Fetching quote for {Symbol} from Yahoo Finance", symbol);

            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to fetch quote for {Symbol}. Status Code: {StatusCode}", symbol, response.StatusCode);
                return null;
            }

            var root = await response.ReadAsJsonAsync<JsonElement>();

            // Parse Yahoo Finance response
            if (!root.TryGetProperty("quoteSummary", out var quoteSummary) || 
                !quoteSummary.TryGetProperty("result", out var results))
            {
                _logger.LogWarning("Unexpected response structure for symbol {Symbol}", symbol);
                return null;
            }

            var result = results.EnumerateArray().FirstOrDefault();
            if (result.ValueKind == JsonValueKind.Undefined)
            {
                _logger.LogWarning("No results found for symbol {Symbol}", symbol);
                return null;
            }

            if (!result.TryGetProperty("price", out var price))
            {
                _logger.LogWarning("Price data not found for symbol {Symbol}", symbol);
                return null;
            }

            return new StockQuoteDto
            {
                Symbol = GetJsonProperty(price, "symbol") ?? symbol,
                Price = GetDecimalProperty(price, "regularMarketPrice") ?? 0,
                Change = GetDecimalProperty(price, "regularMarketChange"),
                ChangePercent = GetDecimalProperty(price, "regularMarketChangePercent"),
                Volume = GetLongProperty(price, "regularMarketVolume"),
                Timestamp = DateTime.UtcNow,
                Currency = GetJsonProperty(price, "currency") ?? "USD"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching quote for symbol {Symbol}", symbol);
            return null;
        }
    }

    public async Task<CompanyInfoDto?> GetCompanyInfoAsync(string symbol)
    {
        try
        {
            var url = $"{_settings.BaseUrl}/api/stock/get-details?region={_settings.Region}&symbol={symbol}";

            _logger.LogInformation("Fetching company info for {Symbol} from Yahoo Finance", symbol);

            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to fetch company info for {Symbol}. Status Code: {StatusCode}", 
                    symbol, response.StatusCode);
                return null;
            }

            var root = await response.ReadAsJsonAsync<JsonElement>();

            // Parse company details from Yahoo Finance response
            if (!root.TryGetProperty("quoteSummary", out var quoteSummary) || 
                !quoteSummary.TryGetProperty("result", out var results))
            {
                _logger.LogWarning("Unexpected response structure for company info {Symbol}", symbol);
                return null;
            }

            var result = results.EnumerateArray().FirstOrDefault();
            if (result.ValueKind == JsonValueKind.Undefined)
            {
                _logger.LogWarning("No company info found for symbol {Symbol}", symbol);
                return null;
            }

            // Get asset profile and price data
            var profile = result.TryGetProperty("assetProfile", out var assetProfile) 
                ? assetProfile 
                : new JsonElement();
            
            var price = result.TryGetProperty("price", out var priceData) 
                ? priceData 
                : new JsonElement();

            return new CompanyInfoDto
            {
                Symbol = symbol,
                Name = GetJsonProperty(price, "longName") ?? GetJsonProperty(price, "shortName") ?? symbol,
                Exchange = GetJsonProperty(price, "exchangeName") ?? GetJsonProperty(price, "exchange"),
                Sector = GetJsonProperty(profile, "sector"),
                Industry = GetJsonProperty(profile, "industry"),
                Description = GetJsonProperty(profile, "longBusinessSummary"),
                Currency = GetJsonProperty(price, "currency") ?? "USD",
                Country = GetJsonProperty(profile, "country")
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching company info for symbol {Symbol}", symbol);
            return null;
        }
    }

    public async Task<List<ExternalSecuritySearchDto>> SearchSecuritiesAsync(string query, int limit = 10)
    {
        try
        {
            var url = $"{_settings.BaseUrl}/api/autocomplete?region={_settings.Region}&search={Uri.EscapeDataString(query)}";

            _logger.LogInformation("Searching for securities with query: {Query}", query);

            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to search securities. Query: {Query}, Status Code: {StatusCode}", 
                    query, response.StatusCode);
                return new List<ExternalSecuritySearchDto>();
            }

            var root = await response.ReadAsJsonAsync<JsonElement>();

            if (!root.TryGetProperty("ResultSet", out var resultSet) || 
                !resultSet.TryGetProperty("Result", out var resultsArray))
            {
                _logger.LogWarning("Unexpected search response structure for query: {Query}", query);
                return new List<ExternalSecuritySearchDto>();
            }

            var results = new List<ExternalSecuritySearchDto>();

            foreach (var item in resultsArray.EnumerateArray().Take(limit))
            {
                results.Add(new ExternalSecuritySearchDto
                {
                    Symbol = GetJsonProperty(item, "symbol") ?? string.Empty,
                    Name = GetJsonProperty(item, "name") ?? GetJsonProperty(item, "longname") ?? string.Empty,
                    Type = GetJsonProperty(item, "quoteType") ?? GetJsonProperty(item, "type"),
                    Region = GetJsonProperty(item, "exchDisp") ?? GetJsonProperty(item, "exchange"),
                    Currency = "USD" // Yahoo Finance search doesn't always return currency
                });
            }

            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching securities with query {Query}", query);
            return new List<ExternalSecuritySearchDto>();
        }
    }

    public async Task<List<HistoricalPriceDto>?> GetHistoricalPricesAsync(string symbol, DateTime startDate, DateTime endDate)
    {
        try
        {
            // Convert dates to Unix timestamps
            var startTimestamp = new DateTimeOffset(startDate).ToUnixTimeSeconds();
            var endTimestamp = new DateTimeOffset(endDate).ToUnixTimeSeconds();

            var url = $"{_settings.BaseUrl}/api/stock/get-historical-data?region={_settings.Region}&symbol={symbol}" +
                     $"&period1={startTimestamp}&period2={endTimestamp}&interval=1d";

            _logger.LogInformation("Fetching historical prices for {Symbol} from {StartDate} to {EndDate}", 
                symbol, startDate, endDate);

            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to fetch historical prices for {Symbol}. Status Code: {StatusCode}", 
                    symbol, response.StatusCode);
                return null;
            }

            var root = await response.ReadAsJsonAsync<JsonElement>();

            // Parse historical data response
            if (!root.TryGetProperty("prices", out var pricesArray))
            {
                _logger.LogWarning("No historical data found for symbol {Symbol}", symbol);
                return null;
            }

            var prices = new List<HistoricalPriceDto>();

            foreach (var entry in pricesArray.EnumerateArray())
            {
                // Skip entries without required data (dividends, splits, etc.)
                if (!entry.TryGetProperty("open", out _))
                    continue;

                var timestamp = GetLongProperty(entry, "date");
                if (!timestamp.HasValue)
                    continue;

                var date = DateTimeOffset.FromUnixTimeSeconds(timestamp.Value).DateTime;

                prices.Add(new HistoricalPriceDto
                {
                    Date = date,
                    Open = GetDecimalProperty(entry, "open") ?? 0,
                    High = GetDecimalProperty(entry, "high") ?? 0,
                    Low = GetDecimalProperty(entry, "low") ?? 0,
                    Close = GetDecimalProperty(entry, "close") ?? 0,
                    Volume = GetLongProperty(entry, "volume") ?? 0
                });
            }

            return prices.OrderBy(p => p.Date).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching historical prices for symbol {Symbol}", symbol);
            return null;
        }
    }

    #region Helper Methods for JSON Parsing

    /// <summary>
    /// Gets a string property from a JSON element
    /// </summary>
    private static string? GetJsonProperty(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var prop))
            return null;

        return prop.ValueKind == JsonValueKind.String 
            ? prop.GetString() 
            : null;
    }

    /// <summary>
    /// Gets a decimal property, handling both raw values and nested "raw" properties
    /// Yahoo Finance often returns: { "raw": 123.45, "fmt": "123.45" }
    /// </summary>
    private static decimal? GetDecimalProperty(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var prop))
            return null;

        // Case 1: Direct number value
        if (prop.ValueKind == JsonValueKind.Number)
        {
            return prop.TryGetDecimal(out var value) ? value : null;
        }

        // Case 2: Nested object with "raw" property
        if (prop.ValueKind == JsonValueKind.Object && prop.TryGetProperty("raw", out var rawValue))
        {
            if (rawValue.ValueKind == JsonValueKind.Number)
            {
                return rawValue.TryGetDecimal(out var value) ? value : null;
            }
        }

        // Case 3: String representation
        if (prop.ValueKind == JsonValueKind.String)
        {
            var str = prop.GetString();
            return decimal.TryParse(str, out var value) ? value : null;
        }

        return null;
    }

    /// <summary>
    /// Gets a long property, handling both raw values and nested "raw" properties
    /// </summary>
    private static long? GetLongProperty(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var prop))
            return null;

        // Case 1: Direct number value
        if (prop.ValueKind == JsonValueKind.Number)
        {
            return prop.TryGetInt64(out var value) ? value : null;
        }

        // Case 2: Nested object with "raw" property
        if (prop.ValueKind == JsonValueKind.Object && prop.TryGetProperty("raw", out var rawValue))
        {
            if (rawValue.ValueKind == JsonValueKind.Number)
            {
                return rawValue.TryGetInt64(out var value) ? value : null;
            }
        }

        // Case 3: String representation
        if (prop.ValueKind == JsonValueKind.String)
        {
            var str = prop.GetString();
            return long.TryParse(str, out var value) ? value : null;
        }

        return null;
    }

    #endregion
}