using PortfolioTracker.Core.Entities;

namespace PortfolioTracker.Core.Interfaces.Repositories;

/// <summary>
/// Repository interface for Holding-specific data operations.
/// Provides data access methods specific to holdings beyond base repository functionality.
/// </summary>
public interface IHoldingRepository : IRepository<Holding>
{
    /// <summary>
    /// Get all holdings for a specific portfolio with security details.
    /// Includes Security navigation property for efficient querying.
    /// </summary>
    Task<IEnumerable<Holding>> GetByPortfolioIdAsync(Guid portfolioId);

    /// <summary>
    /// Get a specific holding with security and portfolio details.
    /// Used when full holding context is needed.
    /// </summary>
    Task<Holding?> GetByIdWithDetailsAsync(Guid holdingId);

    /// <summary>
    /// Get holding by portfolio and security (ensures user owns portfolio).
    /// Useful for checking if holding already exists before creating.
    /// </summary>
    Task<Holding?> GetByPortfolioAndSecurityAsync(Guid portfolioId, Guid securityId);

    /// <summary>
    /// Check if a holding exists in a specific portfolio.
    /// Used for validation before operations.
    /// </summary>
    Task<bool> ExistsInPortfolioAsync(Guid holdingId, Guid portfolioId);

    /// <summary>
    /// Get holdings with transaction history.
    /// Used for detailed position analysis.
    /// </summary>
    Task<IEnumerable<Holding>> GetWithTransactionsAsync(Guid portfolioId);
}
