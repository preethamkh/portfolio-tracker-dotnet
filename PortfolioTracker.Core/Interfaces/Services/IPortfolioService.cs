using PortfolioTracker.Core.DTOs.Portfolio;

namespace PortfolioTracker.Core.Interfaces.Services;

/// <summary>
/// Service interface for portfolio management operations.
/// Defines the contract for business logic related to portfolios.
/// </summary>
public interface IPortfolioService
{
    /// <summary>
    /// Get all portfolios for a specific user.
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>List of user's portfolios</returns>
    Task<IEnumerable<PortfolioDto>> GetUserPortfoliosAsync(Guid userId);

    /// <summary>
    /// Get a specific portfolio by ID.
    /// Ensures the portfolio belongs to the specified user (authorization).
    /// </summary>
    /// <param name="portfolioId">Portfolio ID</param>
    /// <param name="userId">User ID (for authorization)</param>
    /// <returns>Portfolio details or null if not found/unauthorized</returns>
    Task<PortfolioDto?> GetPortfolioByIdAsync(Guid portfolioId, Guid userId);

    /// <summary>
    /// Get the user's default portfolio.
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>Default portfolio or null if none set</returns>
    Task<PortfolioDto?> GetDefaultPortfolioAsync(Guid userId);

    /// <summary>
    /// Create a new portfolio for a user.
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="createPortfolioDto">Portfolio creation data</param>
    /// <returns>Created portfolio details</returns>
    Task<PortfolioDto> CreatePortfolioAsync(Guid userId, CreatePortfolioDto createPortfolioDto);

    /// <summary>
    /// Update portfolio details.
    /// </summary>
    /// <param name="portfolioId">Portfolio ID</param>
    /// <param name="userId">User ID (for authorization)</param>
    /// <param name="updatePortfolioDto">Updated portfolio data</param>
    /// <returns>Updated portfolio details or null if not found/unauthorized</returns>
    Task<PortfolioDto?> UpdatePortfolioAsync(Guid portfolioId, Guid userId, UpdatePortfolioDto updatePortfolioDto);

    /// <summary>
    /// Delete a portfolio.
    /// </summary>
    /// <param name="portfolioId">Portfolio ID</param>
    /// <param name="userId">User ID (for authorization)</param>
    /// <returns>True if deleted, false if not found/unauthorized</returns>
    Task<bool> DeletePortfolioAsync(Guid portfolioId, Guid userId);

    /// <summary>
    /// Set a portfolio as the user's default.
    /// </summary>
    /// <param name="portfolioId">Portfolio ID</param>
    /// <param name="userId">User ID (for authorization)</param>
    /// <returns>True if set as default, false if not found/unauthorized</returns>
    Task<bool> SetAsDefaultAsync(Guid portfolioId, Guid userId);
}