namespace PortfolioTracker.Core.DTOs.ExternalData;

public class ExternalSecuritySearchDto
{
    public string Symbol { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Type { get; set; }
    public string? Region { get; set; }
    public string? Exchange { get; set; }
    public string Currency { get; set; } = "USD";
}
