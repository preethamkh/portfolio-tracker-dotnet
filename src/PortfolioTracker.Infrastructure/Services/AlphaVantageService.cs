using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PortfolioTracker.Core.Configuration;
using PortfolioTracker.Core.DTOs.ExternalData;
using PortfolioTracker.Core.Interfaces.Services;
using System.Text.Json;

namespace PortfolioTracker.Infrastructure.Services;

public class AlphaVantageService(
    HttpClient httpClient,
    IOptions<AlphaVantageSettings> settings,
    ILogger<AlphaVantageService> logger)
    : IStockDataService
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly ILogger<AlphaVantageService> _logger = logger;
    private readonly string _apiKey = settings.Value.ApiKey;
    private readonly string _baseUrl = settings.Value.BaseUrl;

    public async Task<StockQuoteDto?> GetQuoteAsync(string symbol)
    {
        try
        {
            var url = $"{_baseUrl}?function=GLOBAL_QUOTE&symbol={symbol}&apikey={_apiKey}";
            var response = await _httpClient.GetStringAsync(url);

            using var doc = JsonDocument.Parse(response);
            var root = doc.RootElement;

            // Check for rate limit or error
            if (root.TryGetProperty("Note", out _) || root.TryGetProperty("Error Message", out _))
            {
                _logger.LogWarning("Alpha Vantage API limit or error for symbol {Symbol}", symbol);
                return null;
            }

            if (!root.TryGetProperty("Global Quote", out var quote))
            {
                _logger.LogWarning("No quote data found for symbol {Symbol}", symbol);
                return null;
            }

            return new StockQuoteDto
            {
                Symbol = GetJsonProperty(quote, "01. symbol"),
                Price = decimal.Parse(GetJsonProperty(quote, "05. price")),
                Change = decimal.TryParse(GetJsonProperty(quote, "09. change"), out var change)
                    ? change : null,
                ChangePercent = ParsePercentage(GetJsonProperty(quote, "10. change percent")),
                Volume = long.TryParse(GetJsonProperty(quote, "06. volume"), out var vol)
                    ? vol : null,
                Timestamp = DateTime.UtcNow,
                Currency = "USD"
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
            var url = $"{_baseUrl}?function=OVERVIEW&symbol={symbol}&apikey={_apiKey}";
            var response = await _httpClient.GetStringAsync(url);

            using var doc = JsonDocument.Parse(response);
            var root = doc.RootElement;

            if (root.TryGetProperty("Note", out _) || !root.TryGetProperty("Symbol", out _))
            {
                _logger.LogWarning("No company info found for symbol {Symbol}", symbol);
                return null;
            }

            return new CompanyInfoDto
            {
                Symbol = GetJsonProperty(root, "Symbol"),
                Name = GetJsonProperty(root, "Name"),
                Exchange = GetJsonProperty(root, "Exchange"),
                Sector = GetJsonProperty(root, "Sector"),
                Industry = GetJsonProperty(root, "Industry"),
                Description = GetJsonProperty(root, "Description"),
                Currency = GetJsonProperty(root, "Currency") ?? "USD",
                Country = GetJsonProperty(root, "Country")
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
            var url = $"{_baseUrl}?function=SYMBOL_SEARCH&keywords={query}&apikey={_apiKey}";
            var response = await _httpClient.GetStringAsync(url);

            using var doc = JsonDocument.Parse(response);
            var root = doc.RootElement;

            if (!root.TryGetProperty("bestMatches", out var matches))
            {
                return new List<ExternalSecuritySearchDto>();
            }

            var results = new List<ExternalSecuritySearchDto>();
            foreach (var match in matches.EnumerateArray().Take(limit))
            {
                results.Add(new ExternalSecuritySearchDto
                {
                    Symbol = GetJsonProperty(match, "1. symbol"),
                    Name = GetJsonProperty(match, "2. name"),
                    Type = GetJsonProperty(match, "3. type"),
                    Region = GetJsonProperty(match, "4. region"),
                    Currency = GetJsonProperty(match, "8. currency") ?? "AUD"
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

    public async Task<List<HistoricalPriceDto>?> GetHistoricalPricesAsync(string symbol, DateTime startDate,
        DateTime endDate)
    {
        try
        {
            // Alpha Vantage returns full history, we filter after
            var url = $"{_baseUrl}?function=TIME_SERIES_DAILY&symbol={symbol}&outputsize=full&apikey={_apiKey}";
            var response = await _httpClient.GetStringAsync(url);

            using var doc = JsonDocument.Parse(response);
            var root = doc.RootElement;

            if (!root.TryGetProperty("Time Series (Daily)", out var timeSeries))
            {
                _logger.LogWarning("No historical data found for symbol {Symbol}", symbol);
                return null;
            }

            var prices = new List<HistoricalPriceDto>();

            foreach (var entry in timeSeries.EnumerateObject())
            {
                if (!DateTime.TryParse(entry.Name, out var date))
                    continue;

                if (date < startDate || date > endDate)
                    continue;

                var data = entry.Value;
                prices.Add(new HistoricalPriceDto
                {
                    Date = date,
                    Open = decimal.Parse(GetJsonProperty(data, "1. open")),
                    High = decimal.Parse(GetJsonProperty(data, "2. high")),
                    Low = decimal.Parse(GetJsonProperty(data, "3. low")),
                    Close = decimal.Parse(GetJsonProperty(data, "4. close")),
                    Volume = long.Parse(GetJsonProperty(data, "5. volume"))
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

    // Helper methods
    private static string GetJsonProperty(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var prop)
            ? prop.GetString() ?? string.Empty
            : string.Empty;
    }

    private static decimal? ParsePercentage(string value)
    {
        if (string.IsNullOrEmpty(value)) return null;

        // Remove % sign if present
        var cleaned = value.Replace("%", "").Trim();
        return decimal.TryParse(cleaned, out var result) ? result : null;
    }
}
