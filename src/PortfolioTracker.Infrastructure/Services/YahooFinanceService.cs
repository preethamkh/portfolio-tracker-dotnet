using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PortfolioTracker.Core.DTOs.ExternalData;
using PortfolioTracker.Core.Helpers;
using PortfolioTracker.Core.Interfaces.Services;
using PortfolioTracker.Infrastructure.Configuration;
using System.Text.Json;

namespace PortfolioTracker.Infrastructure.Services;

/// <summary>
/// Yahoo Finance API implementation via RapidAPI (yahoo-finance166).
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
        _httpClient.DefaultRequestHeaders.Add("x-rapidapi-key", _settings.ApiKey);
        _httpClient.DefaultRequestHeaders.Add("x-rapidapi-host", _settings.RapidApiHost);
    }

    public async Task<StockQuoteDto?> GetQuoteAsync(string symbol)
    {
        try
        {
            // /market/v2/get-quotes
            var url = $"{_settings.BaseUrl}/market/v2/get-quotes?region={_settings.Region}&symbols={symbol}";

            _logger.LogInformation("Fetching quote for {Symbol} from Yahoo Finance", symbol);

            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Failed to fetch quote for {Symbol}. Status: {StatusCode}, Response: {Response}",
                    symbol, response.StatusCode, errorContent);
                return null;
            }

            var root = await response.ReadAsJsonAsync<JsonElement>();

            // Response structure: { "quoteResponse": { "result": [...], "error": null } }
            if (!root.TryGetProperty("quoteResponse", out var quoteResponse) ||
                !quoteResponse.TryGetProperty("result", out var results))
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

            return new StockQuoteDto
            {
                Symbol = GetJsonProperty(result, "symbol") ?? symbol,
                Price = GetDecimalProperty(result, "regularMarketPrice") ?? 0,
                Change = GetDecimalProperty(result, "regularMarketChange"),
                ChangePercent = GetDecimalProperty(result, "regularMarketChangePercent"),
                Volume = GetLongProperty(result, "regularMarketVolume"),
                Timestamp = DateTime.UtcNow,
                Currency = GetJsonProperty(result, "currency") ?? "USD"
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
            // /stock/v2/get-profile
            var url = $"{_settings.BaseUrl}/stock/v2/get-profile?symbol={symbol}&region={_settings.Region}";

            _logger.LogInformation("Fetching company info for {Symbol} from Yahoo Finance", symbol);

            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Failed to fetch company info for {Symbol}. Status: {StatusCode}, Response: {Response}",
                    symbol, response.StatusCode, errorContent);
                return null;
            }

            var root = await response.ReadAsJsonAsync<JsonElement>();

            // Response structure: { "assetProfile": {...}, "price": {...} }
            var profile = root.TryGetProperty("assetProfile", out var assetProfile)
                ? assetProfile
                : new JsonElement();

            var price = root.TryGetProperty("price", out var priceData)
                ? priceData
                : new JsonElement();

            // Get symbol from price or root
            var returnedSymbol = GetJsonProperty(price, "symbol") ?? GetJsonProperty(root, "symbol") ?? symbol;

            return new CompanyInfoDto
            {
                Symbol = returnedSymbol,
                Name = GetJsonProperty(price, "longName") ?? GetJsonProperty(price, "shortName") ?? returnedSymbol,
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
            // /auto-complete
            var url = $"{_settings.BaseUrl}/auto-complete?q={Uri.EscapeDataString(query)}&region={_settings.Region}";

            _logger.LogInformation("Searching for securities with query: {Query}", query);

            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Failed to search securities. Query: {Query}, Status: {StatusCode}, Response: {Response}",
                    query, response.StatusCode, errorContent);
                return new List<ExternalSecuritySearchDto>();
            }

            var root = await response.ReadAsJsonAsync<JsonElement>();

            // Response structure: { "quotes": [...] }
            if (!root.TryGetProperty("quotes", out var quotesArray))
            {
                _logger.LogWarning("Unexpected search response structure for query: {Query}", query);
                return new List<ExternalSecuritySearchDto>();
            }

            var results = new List<ExternalSecuritySearchDto>();

            foreach (var item in quotesArray.EnumerateArray().Take(limit))
            {
                var symbol = GetJsonProperty(item, "symbol");
                if (string.IsNullOrEmpty(symbol))
                    continue;

                results.Add(new ExternalSecuritySearchDto
                {
                    Symbol = symbol,
                    Name = GetJsonProperty(item, "longname") ?? GetJsonProperty(item, "shortname") ?? symbol,
                    Type = GetJsonProperty(item, "quoteType") ?? GetJsonProperty(item, "typeDisp"),
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
            // /stock/v3/get-historical-data
            var url = $"{_settings.BaseUrl}/stock/v3/get-historical-data?symbol={symbol}&region={_settings.Region}";

            _logger.LogInformation("Fetching historical prices for {Symbol} from {StartDate} to {EndDate}",
                symbol, startDate, endDate);

            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Failed to fetch historical prices for {Symbol}. Status: {StatusCode}, Response: {Response}",
                    symbol, response.StatusCode, errorContent);
                return null;
            }

            var root = await response.ReadAsJsonAsync<JsonElement>();

            // Response structure: { "prices": [...], "isPending": false, ... }
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

                // Filter by date range
                if (date < startDate || date > endDate)
                    continue;

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