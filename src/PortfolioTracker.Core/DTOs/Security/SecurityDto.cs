namespace PortfolioTracker.Core.DTOs.Security;

/// <summary>
/// DTO for returning security information.
/// Securities are master data - one record per stock/ETF shared by all users.
/// </summary>
public class SecurityDto
{
    public Guid Id { get; set; }

    /// <summary>
    /// Trading symbol (e.g., AAPL, MSFT, VAS.AX)
    /// </summary>
    public string Symbol { get; set; } = string.Empty;

    /// <summary>
    /// Full company/fund name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Stock exchange (e.g., NASDAQ, NYSE, ASX)
    /// </summary>
    public string? Exchange { get; set; }

    /// <summary>
    /// Type of security (STOCK, ETF, CRYPTO, etc.)
    /// </summary>
    public string SecurityType { get; set; } = string.Empty;

    /// <summary>
    /// Trading currency (USD, AUD, etc.)
    /// </summary>
    // todo: might have to switch to USD since the APIs mostly support USD
    public string Currency { get; set; } = "AUD";

    /// <summary>
    /// Business sector (Technology, Healthcare, etc.)
    /// </summary>
    public string? Sector { get; set; }

    /// <summary>
    /// Industry classification
    /// </summary>
    public string? Industry { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
