namespace BlazorFrontend.Models;

/// <summary>
/// Security master data (one record per stock/ETF shared by all users).
/// Mirrors the backend's SecurityDto.
/// </summary>
public class Security
{
    public Guid Id { get; set; }
    public string Symbol { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Exchange { get; set; }
    public string SecurityType { get; set; } = string.Empty;
    public string Currency { get; set; } = "AUD";
    public string? Sector { get; set; }
    public string? Industry { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
