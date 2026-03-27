namespace PortfolioTracker.Web.Models.ViewModels.Holdings;

public class HoldingViewModel
{
    public int Id { get; set; }
    public int PortfolioId { get; set; }
    public string PortfolioName { get; set; } = string.Empty;
    public string Symbol { get; set; } = string.Empty;
    public string SecurityName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal AverageCost { get; set; }
    public decimal CurrentPrice { get; set; }
    public decimal MarketValue => Quantity * CurrentPrice;
    public decimal CostBasis => Quantity * AverageCost;
    public decimal GainLoss => MarketValue - CostBasis;
    public decimal GainLossPercent => CostBasis != 0 ? (GainLoss / CostBasis) * 100 : 0;
}
