namespace PortfolioTracker.Web.Models.ViewModels.Transactions;

public class TransactionViewModel
{
    public int Id { get; set; }
    public int HoldingId { get; set; }
    public string Symbol { get; set; } = string.Empty;
    public string TransactionType { get; set; } = string.Empty; // "Buy" or "Sell"
    public decimal Quantity { get; set; }
    public decimal Price { get; set; }
    public decimal Total => Quantity * Price;
    public decimal Fees { get; set; }
    public DateTime TransactionDate { get; set; }
    public string? Notes { get; set; }
}
