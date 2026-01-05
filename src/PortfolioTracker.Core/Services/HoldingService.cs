using Microsoft.Extensions.Logging;
using PortfolioTracker.Core.DTOs.Holding;
using PortfolioTracker.Core.Entities;
using PortfolioTracker.Core.Interfaces.Repositories;
using PortfolioTracker.Core.Interfaces.Services;

namespace PortfolioTracker.Core.Services;

public class HoldingService : IHoldingService
{
    private readonly IHoldingRepository _holdingRepository;
    private readonly IPortfolioRepository _portfolioRepository;
    private readonly ISecurityRepository _securityRepository;
    private readonly IStockDataService _stockDataService;
    private readonly ILogger<HoldingService> _logger;

    public HoldingService(IHoldingRepository holdingRepository, IPortfolioRepository portfolioRepository, ISecurityRepository securityRepository, IStockDataService stockDataService, ILogger<HoldingService> logger)
    {
        _holdingRepository = holdingRepository;
        _portfolioRepository = portfolioRepository;
        _securityRepository = securityRepository;
        _stockDataService = stockDataService;
        _logger = logger;
    }

    public async Task<IEnumerable<HoldingDto>> GetPortfolioHoldingsAsync(Guid portfolioId, Guid userId)
    {
        // Verify user owns the portfolio
        var portfolio = await _portfolioRepository.GetByIdAndUserIdAsync(portfolioId, userId);

        if (portfolio == null)
        {
            _logger.LogWarning("Portfolio {PortfolioId} not found for user {UserId}", portfolioId, userId);
            return Enumerable.Empty<HoldingDto>();
        }

        var holdings = await _holdingRepository.GetByPortfolioIdAsync(portfolioId);

        var holdingDtos = new List<HoldingDto>();

        foreach (var holding in holdings)
        {
            var holdingDto = await MapToHoldingDtoAsync(holding);
            holdingDtos.Add(holdingDto);
        }

        return holdingDtos;
    }

    public async Task<HoldingDto?> GetHoldingByIdAsync(Guid holdingId, Guid portfolioId, Guid userId)
    {
        // Verify user owns the portfolio
        var portfolio = await _portfolioRepository.GetByIdAndUserIdAsync(portfolioId, userId);
        if (portfolio == null)
        {
            _logger.LogWarning("Portfolio {PortfolioId} not found for user {UserId}", portfolioId, userId);
            return null;
        }

        // Verify holding exists in portfolio
        var holdingExists = await _holdingRepository.ExistsInPortfolioAsync(holdingId, portfolioId);
        if (!holdingExists)
        {
            _logger.LogWarning("Holding {HoldingId} not found in portfolio {PortfolioId}", holdingId, portfolioId);
            return null;
        }

        var holding = await _holdingRepository.GetByIdWithDetailsAsync(holdingId);
        if (holding == null) return null;

        return await MapToHoldingDtoAsync(holding);
    }

    public async Task<HoldingDto?> CreateHoldingAsync(Guid portfolioId, Guid userId, CreateHoldingDto createHoldingDto)
    {
        // Verify user owns the portfolio
        var portfolio = await _portfolioRepository.GetByIdAndUserIdAsync(portfolioId, userId);
        if (portfolio == null)
        {
            throw new InvalidOperationException($"Portfolio {portfolioId} not found or user does not have access");
        }

        // Verify security exists
        var security = await _securityRepository.GetByIdAsync(createHoldingDto.SecurityId);
        if (security == null)
        {
            throw new InvalidOperationException($"Security {createHoldingDto.SecurityId} not found");
        }

        // Check if holding for this security already exists in the portfolio
        var existingHolding =
            await _holdingRepository.GetByPortfolioAndSecurityAsync(portfolioId, createHoldingDto.SecurityId);
        if (existingHolding != null)
        {
            throw new InvalidOperationException($"Holding for security {security.Symbol} already exists in this portfolio");
        }

        // Create new holding
        var holding = new Holding
        {
            Id = Guid.NewGuid(),
            PortfolioId = portfolioId,
            SecurityId = createHoldingDto.SecurityId,
            TotalShares = createHoldingDto.TotalShares,
            AverageCost = createHoldingDto.AverageCost,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _holdingRepository.AddAsync(holding);
        await _holdingRepository.SaveChangesAsync();

        _logger.LogInformation(
            "Created holding {HoldingId} for security {Symbol} in portfolio {PortfolioId}",
            holding.Id, security.Symbol, portfolioId);

        // Load with details for DTO mapping
        // Common pattern: fetching the newly added holding from the repository after it has been saved to the database. This is done to return the fully-hydrated DTO, including any related entities that might be needed for the response.
        var createdHolding = await _holdingRepository.GetByIdWithDetailsAsync(holding.Id);
        return await MapToHoldingDtoAsync(createdHolding!);
    }

    public async Task<HoldingDto?> UpdateHoldingAsync(Guid holdingId, Guid portfolioId, Guid userId, UpdateHoldingDto updateHoldingDto)
    {
        // Verify user owns the portfolio
        var portfolio = await _portfolioRepository.GetByIdAndUserIdAsync(portfolioId, userId);
        if (portfolio == null)
        {
            _logger.LogWarning("Portfolio {PortfolioId} not found for user {UserId}", portfolioId, userId);
            return null;
        }

        // Verify holding exists in portfolio
        var holdingExists = await _holdingRepository.ExistsInPortfolioAsync(holdingId, portfolioId);
        if (!holdingExists)
        {
            _logger.LogWarning("Holding {HoldingId} not found in portfolio {PortfolioId}", holdingId, portfolioId);
            return null;
        }

        var holding = await _holdingRepository.GetByIdWithDetailsAsync(holdingId);
        if (holding == null)
        {
            return null;
        }

        // Update holding details
        holding.TotalShares = updateHoldingDto.TotalShares;
        holding.AverageCost = updateHoldingDto.AverageCost;
        holding.UpdatedAt = DateTime.UtcNow;

        await _holdingRepository.UpdateAsync(holding);
        await _holdingRepository.SaveChangesAsync();

        _logger.LogInformation("Updated holding {HoldingId} in portfolio {PortfolioId}", holdingId, portfolioId);

        return await MapToHoldingDtoAsync(holding);
    }

    public async Task<bool> DeleteHoldingAsync(Guid holdingId, Guid portfolioId, Guid userId)
    {
        // Verify user owns the portfolio
        var portfolio = await _portfolioRepository.GetByIdAndUserIdAsync(portfolioId, userId);
        if (portfolio == null)
        {
            _logger.LogWarning("Portfolio {PortfolioId} not found for user {UserId}", portfolioId, userId);
            return false;
        }

        // Verify holding exists in portfolio
        var holdingExists = await _holdingRepository.ExistsInPortfolioAsync(holdingId, portfolioId);
        if (!holdingExists)
        {
            _logger.LogWarning("Holding {HoldingId} not found in portfolio {PortfolioId}", holdingId, portfolioId);
            return false;
        }

        var holding = await _holdingRepository.GetByIdAsync(holdingId);
        if (holding == null)
        {
            return false;
        }

        await _holdingRepository.DeleteAsync(holding);
        await _holdingRepository.SaveChangesAsync();

        _logger.LogInformation("Deleted holding {HoldingId} from portfolio {PortfolioId}", holdingId, portfolioId);

        return true;
    }

    /// <summary>
    /// Maps Holding entity to HoldingDto with current price and valuation.
    /// Fetches real-time price from stock data service (cached).
    /// </summary>
    /// <param name="holding"></param>
    /// <returns></returns>
    private async Task<HoldingDto> MapToHoldingDtoAsync(Holding holding)
    {
        var dto = new HoldingDto
        {
            HoldingId = holding.Id,
            PortfolioId = holding.PortfolioId,
            SecurityId = holding.SecurityId,
            Symbol = holding.Security.Symbol,
            SecurityName = holding.Security.Name,
            SecurityType = holding.Security.SecurityType,
            TotalShares = holding.TotalShares,
            AverageCost = holding.AverageCost,
            CreatedAt = holding.CreatedAt,
            UpdatedAt = holding.UpdatedAt
        };

        // Fetch current price from stock data service (cached)
        try
        {
            var quote = await _stockDataService.GetQuoteAsync(holding.Security.Symbol);
            if (quote != null)
            {
                dto.CurrentPrice = quote.Price;
                dto.CurrentValue = dto.CurrentPrice * dto.TotalShares;
                if (dto.AverageCost.HasValue)
                {
                    dto.TotalCost = dto.AverageCost.Value * dto.TotalShares;
                    dto.UnrealizedGainLoss = dto.CurrentValue - dto.TotalCost;
                    dto.UnrealizedGainLossPercent = dto.TotalCost != 0 ? (dto.UnrealizedGainLoss / dto.TotalCost) * 100 : 0;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to fetch current price for {Symbol}", holding.Security.Symbol);
            // Continue without price data - not a critical error
        }

        return dto;
    }
}
