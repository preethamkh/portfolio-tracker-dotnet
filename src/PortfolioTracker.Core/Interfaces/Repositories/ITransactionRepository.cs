using PortfolioTracker.Core.Entities;

namespace PortfolioTracker.Core.Interfaces.Repositories;

public interface ITransactionRepository : IRepository<Transaction>
{
    /// <summary>
    /// Get all transactions for a specific holding.
    /// </summary>
    Task<IEnumerable<Transaction>> GetByHoldingIdAsync(Guid holdingId);

    /// <summary>
    /// Get transaction with holding and security details.
    /// </summary>
    Task<Transaction?> GetByIdWithDetailsAsync(Guid transactionId);

    /// <summary>
    /// Get all transactions for a portfolio.
    /// Useful for portfolio transaction history.
    /// </summary>
    Task<IEnumerable<Transaction>> GetByPortfolioIdAsync(Guid portfolioId);

    /// <summary>
    /// Check if transaction exists in a specific holding.
    /// </summary>
    Task<bool> ExistsInHoldingAsync(Guid transactionId, Guid holdingId);
}
