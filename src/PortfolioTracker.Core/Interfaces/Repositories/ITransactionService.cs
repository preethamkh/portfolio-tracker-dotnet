using PortfolioTracker.Core.DTOs.Transaction;

namespace PortfolioTracker.Core.Interfaces.Repositories;

/// <summary>
/// Service for managing transactions (buy/sell operations).
/// Automatically updates holding positions and calculates average cost.
/// </summary>
public interface ITransactionService
{
    /// <summary>
    /// Get all transactions for a specific holding.
    /// </summary>
    Task<IEnumerable<TransactionDto>> GetHoldingTransactionsAsync(Guid holdingId, Guid userId);

    /// <summary>
    /// Get all transactions for a portfolio.
    /// </summary>
    Task<IEnumerable<TransactionDto>> GetPortfolioTransactionsAsync(Guid portfolioId, Guid userId);

    /// <summary>
    /// Get a specific transaction by ID.
    /// </summary>
    Task<TransactionDto?> GetTransactionByIdAsync(Guid transactionId, Guid userId);

    /// <summary>
    /// Create a new transaction (buy or sell).
    /// Automatically updates holding's TotalShares and AverageCost.
    /// </summary>
    Task<TransactionDto> CreateTransactionAsync(Guid userId, CreateTransactionDto createTransactionDto);

    /// <summary>
    /// Update an existing transaction.
    /// Recalculates holding position.
    /// </summary>
    Task<TransactionDto?> UpdateTransactionAsync(Guid transactionId, Guid userId, UpdateTransactionDto updateTransactionDto);

    /// <summary>
    /// Delete a transaction.
    /// Recalculates holding position.
    /// </summary>
    Task<bool> DeleteTransactionAsync(Guid transactionId, Guid userId);
}
