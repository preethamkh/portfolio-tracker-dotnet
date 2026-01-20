using PortfolioTracker.Core.DTOs.ExternalData;
using PortfolioTracker.Core.DTOs.Security;
using PortfolioTracker.Core.Interfaces.Services;

namespace PortfolioTracker.IntegrationTests.Helpers;

public class MockStockDataService : IStockDataService
{
    public Task<SecurityDto?> GetSecurityBySymbolAsync(string symbol)
    {
        // Return a mock security for testing
        if (symbol == "TSLA")
        {
            return Task.FromResult<SecurityDto?>(new SecurityDto
            {
                Id = Guid.NewGuid(),
                Symbol = "TSLA",
                Name = "Tesla Inc.",
                Exchange = "NASDAQ",
                Currency = "USD"
            });
        }
        
        // Return null for unknown symbols
        return Task.FromResult<SecurityDto?>(null);
    }

    public async Task<StockQuoteDto?> GetQuoteAsync(string symbol)
    {
        // Return a mock stock quote for testing
        if (symbol == "TSLA")
        {
            return await Task.FromResult<StockQuoteDto?>(new StockQuoteDto
            {
                Symbol = "TSLA",
                Price = 700.00m,
                Change = 5.00m,
                ChangePercent = 0.72m,
                Volume = 20000000,
                Timestamp = DateTime.UtcNow,
                Currency = "USD"
            });
        }

        if (symbol == "MSFT")
        {
            return await Task.FromResult<StockQuoteDto?>(new StockQuoteDto
            {
                Symbol = "MSFT",
                Price = 700.00m,
                Change = 5.00m,
                ChangePercent = 0.72m,
                Volume = 20000000,
                Timestamp = DateTime.UtcNow,
                Currency = "USD"
            });
        }

        if (symbol == "AAPL")
        {
            return await Task.FromResult<StockQuoteDto?>(new StockQuoteDto
            {
                Symbol = "AAPL",
                Price = 700.00m,
                Change = 5.00m,
                ChangePercent = 0.72m,
                Volume = 20000000,
                Timestamp = DateTime.UtcNow,
                Currency = "USD"
            });
        }

        // Return null for unknown symbols
        return await Task.FromResult<StockQuoteDto?>(null);
    }

    public async Task<CompanyInfoDto?> GetCompanyInfoAsync(string symbol)
    {
        // Return a mock company info for testing
        if (symbol == "TSLA")
        {
            return await Task.FromResult<CompanyInfoDto?>(new CompanyInfoDto
            {
                Symbol = "TSLA",
                Name = "Tesla Inc.",
                Industry = "Automotive",
                Description = "Tesla, Inc. designs, develops, manufactures, and sells electric vehicles and energy generation and storage systems.",
                Sector = "Consumer Cyclical",
                Country = "USA"
            });
        }

        if (symbol == "MSFT")
        {
            return await Task.FromResult<CompanyInfoDto?>(new CompanyInfoDto
            {
                Symbol = "MSFT",
                Name = "Microsoft",
                Industry = "Computer",
                Description = "Microsoft Corporation is an American multinational technology corporation that produces computer software, consumer electronics, personal computers, and related services.",
                Sector = "Technology",
                Country = "USA"
            });
        }

        if (symbol == "AAPL")
        {
            return await Task.FromResult<CompanyInfoDto?>(new CompanyInfoDto
            {
                Symbol = "AAPL",
                Name = "Apple Inc.",
                Industry = "Computer",
                Description = "Apple Inc. is an American multinational technology company that designs, manufactures, and markets consumer electronics, computer software, and online services.",
                Sector = "Technology",
                Country = "USA"
            });
        }

        // Return null for unknown symbols
        return await Task.FromResult<CompanyInfoDto?>(null);
    }

    public async Task<List<ExternalSecuritySearchDto>> SearchSecuritiesAsync(string query, int limit = 10)
    {
        // Return mock search results for testing
        var results = new List<ExternalSecuritySearchDto>
        {
            new()
            {
                Symbol = "TSLA",
                Name = "Tesla Inc.",
                Exchange = "NASDAQ",
                Currency = "USD"
            },
            new()
            {
                Symbol = "AAPL",
                Name = "Apple Inc.",
                Exchange = "NASDAQ",
                Currency = "USD"
            },
            new()
            {
                Symbol = "MSFT",
                Name = "Microsoft",
                Exchange = "NASDAQ",
                Currency = "USD"
            }
        };
        return await Task.FromResult(results.Take(limit).ToList());
    }

    public async Task<List<HistoricalPriceDto>?> GetHistoricalPricesAsync(string symbol, DateTime startDate, DateTime endDate)
    {
        // Return mock historical prices for testing
        if (symbol == "TSLA")
        {
            var prices = new List<HistoricalPriceDto>
            {
                new()
                {
                    Date = startDate,
                    Open = 650.00m,
                    High = 680.00m,
                    Low = 640.00m,
                    Close = 670.00m,
                    Volume = 15000000
                },
                new()
                {
                    Date = endDate,
                    Open = 680.00m,
                    High = 700.00m,
                    Low = 670.00m,
                    Close = 690.00m,
                    Volume = 18000000
                }
            };
            return await Task.FromResult(prices);
        }
        // Return null for unknown symbols
        return await Task.FromResult<List<HistoricalPriceDto>?>(null);
    }
}
