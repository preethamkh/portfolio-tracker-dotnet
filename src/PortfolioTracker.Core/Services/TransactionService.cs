using Microsoft.Extensions.Logging;
using PortfolioTracker.Core.DTOs.Transaction;
using PortfolioTracker.Core.Entities;
using PortfolioTracker.Core.Enums;
using PortfolioTracker.Core.Interfaces.Repositories;

namespace PortfolioTracker.Core.Services;

public class TransactionService : ITransactionService
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly IHoldingRepository _holdingRepository;
    private readonly IPortfolioRepository _portfolioRepository;
    private readonly ILogger<TransactionService> _logger;

    public TransactionService(
        ITransactionRepository transactionRepository,
        IHoldingRepository holdingRepository,
        IPortfolioRepository portfolioRepository,
        ILogger<TransactionService> logger)
    {
        _transactionRepository = transactionRepository;
        _holdingRepository = holdingRepository;
        _portfolioRepository = portfolioRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<TransactionDto>> GetHoldingTransactionsAsync(Guid holdingId, Guid userId)
    {
        var holding = await _holdingRepository.GetByIdWithDetailsAsync(holdingId);
        if (holding == null)
        {
            return Enumerable.Empty<TransactionDto>();
        }

        // Verify user owns the portfolio
        var portfolio = await _portfolioRepository.GetByIdAndUserIdAsync(holding.PortfolioId, userId);
        if (portfolio == null)
        {
            _logger.LogWarning("User {UserId} attempted to access transactions for holding {HoldingId}", userId, holdingId);
            return Enumerable.Empty<TransactionDto>();
        }

        var transactions = await _transactionRepository.GetByHoldingIdAsync(holdingId);
        return transactions.Select(MapToTransactionDto);
    }

    public async Task<IEnumerable<TransactionDto>> GetPortfolioTransactionsAsync(Guid portfolioId, Guid userId)
    {
        // Verify user owns the portfolio
        var portfolio = await _portfolioRepository.GetByIdAndUserIdAsync(portfolioId, userId);
        if (portfolio == null)
        {
            return Enumerable.Empty<TransactionDto>();
        }

        var transactions = await _transactionRepository.GetByPortfolioIdAsync(portfolioId);
        return transactions.Select(MapToTransactionDto);
    }

    public async Task<TransactionDto?> GetTransactionByIdAsync(Guid transactionId, Guid userId)
    {
        var transaction = await _transactionRepository.GetByIdWithDetailsAsync(transactionId);
        if (transaction == null)
        {
            return null;
        }

        // Verify user owns the portfolio
        var portfolio = await _portfolioRepository.GetByIdAndUserIdAsync(transaction.Holding.PortfolioId, userId);
        if (portfolio == null)
        {
            _logger.LogWarning("User {UserId} attempted to access transaction {TransactionId}", userId, transactionId);
            return null;
        }

        return MapToTransactionDto(transaction);
    }

    public async Task<TransactionDto> CreateTransactionAsync(Guid userId, Guid portfolioId, CreateTransactionDto createTransactionDto)
    {
        // Validate transaction type
        if (createTransactionDto.TransactionType != TransactionType.Buy && createTransactionDto.TransactionType != TransactionType.Sell)
        {
            throw new InvalidOperationException("Transaction type must be Buy or Sell.");
        }

        // Get holding and verify user owns it
        var holding = await _holdingRepository.GetByIdWithDetailsAsync(createTransactionDto.HoldingId);
        if (holding == null || holding.PortfolioId != portfolioId)
        {
            throw new InvalidOperationException($"Holding {createTransactionDto.HoldingId} not found");
        }

        var portfolio = await _portfolioRepository.GetByIdAndUserIdAsync(portfolioId, userId);
        if (portfolio == null)
        {
            throw new InvalidOperationException("User does not have access to this portfolio");
        }

        // Validate sell transaction
        if (createTransactionDto.TransactionType == TransactionType.Sell && createTransactionDto.Shares > holding.TotalShares)
        {
            throw new InvalidOperationException($"Cannot sell {createTransactionDto.Shares} shares. Only {holding.TotalShares} shares available.");
        }

        // Calculate total amount
        var totalAmount = createTransactionDto.Shares * createTransactionDto.PricePerShare + createTransactionDto.Fees;

        // Create transaction
        var transaction = new Transaction
        {
            Id = Guid.NewGuid(),
            HoldingId = createTransactionDto.HoldingId,
            TransactionType = createTransactionDto.TransactionType,
            Shares = createTransactionDto.Shares,
            PricePerShare = createTransactionDto.PricePerShare,
            TotalAmount = totalAmount,
            Fees = createTransactionDto.Fees,
            TransactionDate = createTransactionDto.TransactionDate,
            Notes = createTransactionDto.Notes,
            CreatedAt = DateTime.UtcNow
        };

        await _transactionRepository.AddAsync(transaction);

        // Update holding
        await UpdateHoldingFromTransactionAsync(holding, createTransactionDto.TransactionType,
            createTransactionDto.Shares, createTransactionDto.PricePerShare, createTransactionDto.Fees);

        await _transactionRepository.SaveChangesAsync();

        _logger.LogInformation("Created {TransactionType} transaction {TransactionId} for holding {HoldingId}",
            transaction.TransactionType, transaction.Id, holding.Id);

        // Reload with details
        var createdTransaction = await _transactionRepository.GetByIdWithDetailsAsync(transaction.Id);
        return MapToTransactionDto(createdTransaction!);
    }

    public async Task<TransactionDto?> UpdateTransactionAsync(
        Guid transactionId,
        Guid userId,
        UpdateTransactionDto updateTransactionDto)
    {
        var transaction = await _transactionRepository.GetByIdWithDetailsAsync(transactionId);
        if (transaction == null)
        {
            return null;
        }

        // Verify user owns the portfolio
        var portfolio = await _portfolioRepository.GetByIdAndUserIdAsync(transaction.Holding.PortfolioId, userId);
        if (portfolio == null)
        {
            _logger.LogWarning("User {UserId} attempted to update transaction {TransactionId}", userId, transactionId);
            return null;
        }

        // Store old values to reverse their effect on holding
        var oldShares = transaction.Shares;
        var oldPricePerShare = transaction.PricePerShare;
        var oldFees = transaction.Fees;
        var transactionType = transaction.TransactionType;

        // Update transaction
        transaction.Shares = updateTransactionDto.Shares;
        transaction.PricePerShare = updateTransactionDto.PricePerShare;
        transaction.Fees = updateTransactionDto.Fees;
        transaction.TotalAmount = updateTransactionDto.Shares * updateTransactionDto.PricePerShare + updateTransactionDto.Fees;
        transaction.TransactionDate = updateTransactionDto.TransactionDate;
        transaction.Notes = updateTransactionDto.Notes;

        await _transactionRepository.UpdateAsync(transaction);

        // Recalculate holding: reverse old transaction, apply new transaction
        var holding = transaction.Holding;
        await ReverseTransactionEffectAsync(holding, transactionType, oldShares, oldPricePerShare, oldFees);
        await UpdateHoldingFromTransactionAsync(holding, transactionType,
            updateTransactionDto.Shares, updateTransactionDto.PricePerShare, updateTransactionDto.Fees);

        await _transactionRepository.SaveChangesAsync();

        _logger.LogInformation("Updated transaction {TransactionId}", transactionId);

        return MapToTransactionDto(transaction);
    }

    public async Task<bool> DeleteTransactionAsync(Guid transactionId, Guid userId)
    {
        var transaction = await _transactionRepository.GetByIdWithDetailsAsync(transactionId);
        if (transaction == null)
        {
            return false;
        }

        // Verify user owns the portfolio
        var portfolio = await _portfolioRepository.GetByIdAndUserIdAsync(transaction.Holding.PortfolioId, userId);
        if (portfolio == null)
        {
            _logger.LogWarning("User {UserId} attempted to delete transaction {TransactionId}", userId, transactionId);
            return false;
        }

        // Reverse transaction effect on holding
        await ReverseTransactionEffectAsync(transaction.Holding, transaction.TransactionType,
            transaction.Shares, transaction.PricePerShare, transaction.Fees);

        await _transactionRepository.DeleteAsync(transaction);
        await _transactionRepository.SaveChangesAsync();

        _logger.LogInformation("Deleted transaction {TransactionId}", transactionId);

        return true;
    }

    /// <summary>
    /// Updates holding based on transaction.
    /// Calculates new TotalShares and AverageCost.
    /// </summary>
    private async Task UpdateHoldingFromTransactionAsync(
        Holding holding,
        TransactionType transactionType,
        decimal shares,
        decimal pricePerShare,
        decimal fees)
    {
        if (transactionType == TransactionType.Buy)
        {
            // Calculate new average cost using weighted average
            var currentValue = holding.TotalShares * (holding.AverageCost ?? 0);
            var newValue = shares * pricePerShare + fees;
            var newTotalShares = holding.TotalShares + shares;

            holding.TotalShares = newTotalShares;
            holding.AverageCost = newTotalShares > 0
                ? (currentValue + newValue) / newTotalShares
                : 0;
        }
        else if (transactionType == TransactionType.Sell)
        {
            holding.TotalShares -= shares;
            // Average cost remains the same on sell
            // If all shares sold, reset average cost
            if (holding.TotalShares == 0)
            {
                holding.AverageCost = null;
            }
        }

        holding.UpdatedAt = DateTime.UtcNow;
        await _holdingRepository.UpdateAsync(holding);
    }

    /// <summary>
    /// Reverses the effect of a transaction on a holding.
    /// Used when updating or deleting transactions.
    /// </summary>
    private async Task ReverseTransactionEffectAsync(
        Holding holding,
        TransactionType transactionType,
        decimal shares,
        decimal pricePerShare,
        decimal fees)
    {
        if (transactionType == TransactionType.Buy)
        {
            // Reverse buy: remove shares and recalculate average cost
            var currentValue = holding.TotalShares * (holding.AverageCost ?? 0);
            var removedValue = shares * pricePerShare + fees;
            var newTotalShares = holding.TotalShares - shares;

            holding.TotalShares = newTotalShares;
            holding.AverageCost = newTotalShares > 0
                ? (currentValue - removedValue) / newTotalShares
                : null;
        }
        else if (transactionType == TransactionType.Sell)
        {
            // Reverse sell: add shares back
            holding.TotalShares += shares;
        }

        holding.UpdatedAt = DateTime.UtcNow;
        await _holdingRepository.UpdateAsync(holding);
    }

    private static TransactionDto MapToTransactionDto(Transaction transaction)
    {
        return new TransactionDto
        {
            Id = transaction.Id,
            HoldingId = transaction.HoldingId,
            Symbol = transaction.Holding.Security.Symbol,
            SecurityName = transaction.Holding.Security.Name,
            TransactionType = transaction.TransactionType,
            Shares = transaction.Shares,
            PricePerShare = transaction.PricePerShare,
            TotalAmount = transaction.TotalAmount,
            Fees = transaction.Fees,
            TransactionDate = transaction.TransactionDate,
            Notes = transaction.Notes,
            CreatedAt = transaction.CreatedAt
        };
    }
}
