namespace PortfolioTracker.Core.DTOs.Transaction;

/// <summary>
/// Transaction details for display.
/// Represents a buy or sell operation.
/// </summary>
public class TransactionDto
{
    public Guid Id { get; set; }
    public Guid HoldingId { get; set; }

    // Security info (denormalized for convenience)
    public string Symbol { get; set; } = string.Empty;
    public string SecurityName { get; set; } = string.Empty;

    // todo: enum?
    public string TransactionType { get; set; } = string.Empty; // "Buy" or "Sell"
    public decimal Shares { get; set; }
    public decimal PricePerShare { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal Fees { get; set; }
    public DateTime TransactionDate { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}
