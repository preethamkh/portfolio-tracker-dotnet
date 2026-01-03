namespace PortfolioTracker.Core.DTOs.ExternalData;

public class StockQuoteDto
{
    public string Symbol { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal? Change { get; set; }
    public decimal? ChangePercent { get; set; }
    public long? Volume { get; set; }
    public DateTime Timestamp { get; set; }
    public string Currency { get; set; } = "AUD";
}
