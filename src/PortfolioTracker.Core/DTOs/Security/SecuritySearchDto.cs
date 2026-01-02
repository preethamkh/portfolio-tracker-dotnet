namespace PortfolioTracker.Core.DTOs.Security;

/// <summary>
/// Lightweight DTO for security search results.
/// Used in autocomplete/search functionality.
/// </summary>
/// <remarks>
/// When user types "APP" in search:
/// [
/// todo: may be solution wide dictionary for unknown acronyms
///   { "symbol": "AAPL", "name": "Apple Inc.", "exchange": "NASDAQ" },
///   { "symbol": "APX", "name": "Appen Ltd", "exchange": "ASX" }
/// ]
/// </remarks>
public class SecuritySearchDto
{
    public Guid Id { get; set; }
    public string Symbol { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Exchange { get; set; }
    public string SecurityType { get; set; } = string.Empty;
}
