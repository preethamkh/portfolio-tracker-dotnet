namespace PortfolioTracker.Core.DTOs.ExternalData;

public class CompanyInfoDto
{
    public string Symbol { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Exchange { get; set; }
    public string? Sector { get; set; }
    public string? Industry { get; set; }
    public string? Description { get; set; }
    public string Currency { get; set; } = "AUD";
    public string? Country { get; set; }
}
