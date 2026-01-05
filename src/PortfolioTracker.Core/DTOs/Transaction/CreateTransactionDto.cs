namespace PortfolioTracker.Core.DTOs.Transaction;

/// <summary>
/// Create a new transaction (buy or sell).
/// Automatically updates holding's TotalShares and AverageCost.
/// </summary>
public class CreateTransactionDto
{
    public Guid HoldingId { get; set; }
    public string TransactionType { get; set; } = string.Empty;
    public decimal Shares { get; set; }
    public decimal PricePerShare { get; set; }
    public decimal Fees { get; set; } = 0;
    public DateTime TransactionDate { get; set; }
    public string? Notes { get; set; }
}
