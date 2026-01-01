using PortfolioTracker.Core.Entities;

namespace PortfolioTracker.Core.Interfaces.Repositories;

/// <summary>
/// Repository interface for Portfolio-specific data operations.
/// Extends generic repository and adds Portfolio-specific methods.
/// </summary>
public interface IPortfolioRepository : IRepository<Portfolio>
{
    /// <summary>
    /// Get all portfolios for a specific user
    /// </summary>
    /// <param name="userId"></param>
    /// <returns>List of user's portfolios</returns>
    Task<IEnumerable<Portfolio>> GetByUserIdAsync(Guid userId);

    /// <summary>
    /// Get a portfolio by its ID, but only if it belongs to the specified user
    /// This method is useful for ensuring that users can only access their own portfolios.
    /// This enforces authorization at the data access layer.
    /// </summary>
    /// <param name="portfolioId"></param>
    /// <param name="userId"></param>
    /// <returns>Portfolio details or null if not found/unauthorized</returns>
    Task<Portfolio?> GetByIdAndUserIdAsync(Guid portfolioId, Guid userId);

    /// <summary>
    /// Get the default portfolio for a specific user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>Default Portfolio or null if none set</returns>
    Task<Portfolio?> GetDefaultPortfolioAsync(Guid userId);

    /// <summary>
    /// Check if a user already has a portfolio with the given name.
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="name">Portfolio name</param>
    /// <param name="excludePortfolioId">Portfolio ID to exclude (for updates)</param>
    Task<bool> UserHasPortfolioWithNameAsync(Guid userId, string name, Guid? excludePortfolioId = null);

    /// <summary>
    /// Set a portfolio as the default for a user.
    /// This will unset any other default portfolio.
    /// </summary>
    /// <param name="portfolioId">Portfolio to set as default</param>
    /// <param name="userId">User ID</param>
    Task SetAsDefaultAsync(Guid portfolioId, Guid userId);

    /// <summary>
    /// Get portfolio with holdings count (for list views).
    /// </summary>
    /// <param name="userId">User ID</param>
    Task<IEnumerable<Portfolio>> GetWithHoldingsCountAsync(Guid userId);
}