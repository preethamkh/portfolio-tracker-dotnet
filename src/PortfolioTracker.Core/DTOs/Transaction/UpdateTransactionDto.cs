namespace PortfolioTracker.Core.DTOs.Transaction;

/// <summary>
/// Update an existing transaction.
/// Recalculates holding position.
/// </summary>
public class UpdateTransactionDto
{
    public decimal Shares { get; set; }
    public decimal PricePerShare { get; set; }
    public decimal Fees { get; set; }
    public DateTime TransactionDate { get; set; }
    public string? Notes { get; set; }
}
