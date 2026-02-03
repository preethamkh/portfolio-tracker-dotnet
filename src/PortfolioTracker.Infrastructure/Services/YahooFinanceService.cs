using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PortfolioTracker.Core.DTOs.ExternalData;
using PortfolioTracker.Core.Helpers;
using PortfolioTracker.Core.Interfaces.Services;
using PortfolioTracker.Infrastructure.Configuration;
using System.Text.Json;

namespace PortfolioTracker.Infrastructure.Services;
public class YahooFinanceService(
    HttpClient httpClient,
    IOptions<YahooFinanceSettings> settings,
    ILogger<YahooFinanceService> logger)
    : IStockDataService
{
    private readonly YahooFinanceSettings _settings = settings.Value;

    public async Task<StockQuoteDto?> GetQuoteAsync(string symbol)
    {
        try
        {
            var url = $"{_settings.BaseUrl}/api/stock/get-price?region={_settings.Region}&symbol={symbol}";

            logger.LogInformation("Fetching quote for {Symbol} from Yahoo Finance", symbol);

            var response = await httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("Failed to fetch quote for {Symbol}. Status Code: {StatusCode}", symbol, response.StatusCode);
                return null;
            }

            var root = await response.ReadAsJsonAsync<JsonElement>();

            // Parse Yahoo Finance response (sample response)
            /*
             *{
                 "quoteSummary": {
                   "result": [
                     {
                       "price": {
                         "regularMarketChange": {
                           "raw": -0.01999998,
                           "fmt": "-0.0200"
                         },
                         "regularMarketPrice": {
                           "raw": 4.89,
                           "fmt": "4.8900"
                         },
                         "regularMarketVolume": {
                           "raw": 19887539,
                           "fmt": "19.89M",
                           "longFmt": "19,887,539.00"
                         },
                         "exchange": "ASX",
                         "quoteType": "EQUITY",
                         "symbol": "TLS.AX",
                         "shortName": "TELSTRA FPO [TLS]",
                         "longName": "Telstra Group Limited",
                         "currency": "AUD",
                         "currencySymbol": "$",
                       }
                     }
                   ],
                   "error": null
                 }
               }
             */

            if (!root.TryGetProperty("quoteSummary", out var quoteSummary) || !quoteSummary.TryGetProperty("result", out var results))
            {
                logger.LogWarning("Unexpected response structure for symbol {Symbol}", symbol);
                return null;
            }

            var result = results.EnumerateArray().FirstOrDefault();
            if (result.ValueKind == JsonValueKind.Undefined)
            {
                logger.LogWarning("No results found for symbol {Symbol}", symbol);
                return null;
            }

            if (!result.TryGetProperty("price", out var price))
            {
                logger.LogWarning("Price data not found for symbol {Symbol}", symbol);
                return null;
            }

            return new StockQuoteDto
            {
                //Symbol = GetJsonProperty(price, "symbol") ?? symbol,
                //Price = decimal.Parse(GetJsonProperty(price, "regularMarketPrice")),
                //Change = decimal.TryParse(GetJsonProperty(quote, "09. change"), out var change)
                //    ? change : null,
                //ChangePercent = GetRawDecimalProperty(GetJsonProperty(quote, "10. change percent")),
                //Volume = long.TryParse(GetJsonProperty(quote, "06. volume"), out var vol)
                //    ? vol : null,
                //Timestamp = DateTime.UtcNow,
                //Currency = "USD"
            };
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task<CompanyInfoDto?> GetCompanyInfoAsync(string symbol)
    {
        throw new NotImplementedException();
    }

    public async Task<List<ExternalSecuritySearchDto>> SearchSecuritiesAsync(string query, int limit = 10)
    {
        throw new NotImplementedException();
    }

    public async Task<List<HistoricalPriceDto>?> GetHistoricalPricesAsync(string symbol, DateTime startDate, DateTime endDate)
    {
        throw new NotImplementedException();
    }

    #region Helper Methods for JSON Parsing

    private static string GetJsonProperty(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var prop)
            ? prop.GetString() ?? string.Empty
            : string.Empty;
    }

    private static decimal? GetDecimalProperty(JsonElement element, string propertyName)
    {
        if (element.TryGetProperty(propertyName, out var prop) && prop.TryGetProperty("raw", out var rawValue))
        {
            if (rawValue.TryGetDecimal(out var decimalValue))
            {
                return decimalValue;
            }
        }
        return null;
    }

    private static decimal? GetRawDecimalProperty(JsonElement element, string propertyName)
    {
        if (element.TryGetProperty(propertyName, out var prop) && prop.TryGetProperty("raw", out var rawValue))
        {
            if (rawValue.TryGetDecimal(out var decimalValue))
            {
                return decimalValue;
            }
        }
        return null;
    }

    private static long? GetLongProperty(JsonElement element, string propertyName)
    {
        if (element.TryGetProperty(propertyName, out var prop) && prop.TryGetProperty("raw", out var rawValue))
        {
            if (rawValue.TryGetInt64(out var longValue))
            {
                return longValue;
            }
        }
        return null;
    }

    private static long? GetRawLongProperty(JsonElement element, string propertyName)
    {
        if (element.TryGetProperty(propertyName, out var prop) && prop.TryGetProperty("raw", out var rawValue))
        {
            if (rawValue.TryGetInt64(out var longValue))
            {
                return longValue;
            }
        }
        return null;
    }

    #endregion
}