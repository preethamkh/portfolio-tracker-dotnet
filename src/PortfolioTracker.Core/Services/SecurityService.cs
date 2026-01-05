using Microsoft.Extensions.Logging;
using PortfolioTracker.Core.DTOs.ExternalData;
using PortfolioTracker.Core.DTOs.Security;
using PortfolioTracker.Core.Entities;
using PortfolioTracker.Core.Interfaces.Repositories;
using PortfolioTracker.Core.Interfaces.Services;

namespace PortfolioTracker.Core.Services;

public class SecurityService : ISecurityService
{
    private readonly ISecurityRepository _securityRepository;
    private readonly IStockDataService _stockDataService;
    private readonly ILogger<SecurityService> _logger;
    public SecurityService(ISecurityRepository securityRepository, IStockDataService stockDataService, ILogger<SecurityService> logger)
    {
        _securityRepository = securityRepository;
        _stockDataService = stockDataService;
        _logger = logger;
    }

    public async Task<List<SecurityDto>> SearchSecuritiesAsync(string query, int limit = 10)
    {
        // Search for securities using cache (Redis) or external API via stock data service
        var externalResults = await _stockDataService.SearchSecuritiesAsync(query, limit);

        var securityDtos = new List<SecurityDto>();

        foreach (var externalSecurity in externalResults)
        {
            // Check if security exists in local database
            var existingSecurity = await _securityRepository.GetBySymbolAsync(externalSecurity.Symbol);
            if (existingSecurity != null)
            {
                // Return from our database
                securityDtos.Add(MapToSecurityDto(existingSecurity));
            }
            else
            {
                // Return from external API (not yet in our DB)
                securityDtos.Add(new SecurityDto
                {
                    Id = Guid.NewGuid(), // Indicates not in our DB yet
                    Symbol = externalSecurity.Symbol,
                    Name = externalSecurity.Name,
                    Exchange = externalSecurity.Exchange,
                    SecurityType = externalSecurity.Type ?? "STOCK",
                    Currency = externalSecurity.Currency,
                    Sector = null,
                    Industry = null,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }
        }

        return securityDtos;
    }

    public async Task<SecurityDto?> GetSecurityByIdAsync(Guid id)
    {
        var security = await _securityRepository.GetByIdAsync(id);
        
        return security != null ? MapToSecurityDto(security) : null;
    }

    public async Task<SecurityDto?> GetSecurityBySymbolAsync(string symbol)
    {
        var security = await _securityRepository.GetBySymbolAsync(symbol);
        return security != null ? MapToSecurityDto(security) : null;
    }

    public async Task<SecurityDto> GetOrCreateSecurityAsync(string symbol)
    {
        // Check if security already exists in our database
        var existingSecurity = await _securityRepository.GetBySymbolAsync(symbol);
        if (existingSecurity != null)
        {
            _logger.LogDebug("Security {Symbol} already exists in database", symbol);
            return MapToSecurityDto(existingSecurity);
        }

        // Fetch company info from external redis/API
        var companyInfo = await _stockDataService.GetCompanyInfoAsync(symbol);
        if (companyInfo == null)
        {
            throw new InvalidOperationException($"Unable to fetch information for symbol {symbol}");
        }

        // Create new Security entity
        var security = new Security
        {
            Id = Guid.NewGuid(),
            Symbol = companyInfo.Symbol.ToUpperInvariant(),
            Name = companyInfo.Name,
            Exchange = companyInfo.Exchange,
            SecurityType = DetermineSecurityType(companyInfo),
            Currency = companyInfo.Currency,
            Sector = companyInfo.Sector,
            Industry = companyInfo.Industry,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _securityRepository.AddAsync(security);
        await _securityRepository.SaveChangesAsync();

        _logger.LogInformation("Created new security {Symbol} in database", symbol);

        return MapToSecurityDto(security);
    }

    private static SecurityDto MapToSecurityDto(Security security)
    {
        return new SecurityDto
        {
            Id = security.Id,
            Symbol = security.Symbol,
            Name = security.Name,
            Exchange = security.Exchange,
            SecurityType = security.SecurityType,
            Currency = security.Currency,
            Sector = security.Sector,
            Industry = security.Industry,
            CreatedAt = security.CreatedAt,
            UpdatedAt = security.UpdatedAt
        };
    }

    /// <summary>
    /// Determines security type from company info.
    /// ETFs usually have "ETF" in the name or type field.
    /// </summary>
    private static string DetermineSecurityType(CompanyInfoDto companyInfo)
    {
        var name = companyInfo.Name.ToUpperInvariant();

        if (name.Contains("ETF") || name.Contains("EXCHANGE TRADED FUND"))
        {
            return "ETF";
        }

        if (name.Contains("FUND") || name.Contains("TRUST"))
        {
            return "Fund";
        }

        // Default to Stock
        return "Stock";
    }
}
