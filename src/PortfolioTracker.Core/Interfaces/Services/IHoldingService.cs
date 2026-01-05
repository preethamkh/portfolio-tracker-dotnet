using PortfolioTracker.Core.DTOs.Holding;

namespace PortfolioTracker.Core.Interfaces.Services;

/// <summary>
/// Service interface for holding business logic operations.
/// Handles holding operations with validation and price enrichment.
/// </summary>
public interface IHoldingService
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="portfolioId"></param>
    /// <param name="userId"></param>
    /// <returns></returns>
    Task<IEnumerable<HoldingDto>> GetPortfolioHoldingsAsync(Guid portfolioId, Guid userId);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="holdingId"></param>
    /// <param name="portfolioId"></param>
    /// <param name="userId"></param>
    /// <returns></returns>
    Task<HoldingDto?> GetHoldingByIdAsync(Guid holdingId, Guid portfolioId, Guid userId);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="portfolioId"></param>
    /// <param name="userId"></param>
    /// <param name="createHoldingDto"></param>
    /// <returns></returns>
    Task<HoldingDto?> CreateHoldingAsync(Guid portfolioId, Guid userId, CreateHoldingDto createHoldingDto);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="holdingId"></param>
    /// <param name="portfolioId"></param>
    /// <param name="userId"></param>
    /// <param name="updateHoldingDto"></param>
    /// <returns></returns>
    Task<HoldingDto?> UpdateHoldingAsync(Guid holdingId, Guid portfolioId, Guid userId, UpdateHoldingDto updateHoldingDto);
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="holdingId"></param>
    /// <param name="portfolioId"></param>
    /// <param name="userId"></param>
    /// <returns></returns>
    Task<bool> DeleteHoldingAsync(Guid holdingId, Guid portfolioId, Guid userId);
}
