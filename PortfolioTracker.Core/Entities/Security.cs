using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PortfolioTracker.Core.Entities;

/// <summary>
/// Represents a security (stock, ETF, or other tradeable asset).
/// This is a master data table - one entry per unique security.
/// </summary>
public class Security
{
    /// <summary>
    /// Unique identifier for the security.
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    /// <summary>
    /// Stock ticker symbol (e.g., "VGS.AX", "GQG", "RMD", "AAPL").
    /// Must be unique across the system.
    /// </summary>
    [Required]
    [MaxLength(20)]
    public string Symbol { get; set; } = string.Empty;

    /// <summary>
    /// Full company/fund name (e.g., "Vanguard MSCI Index International Shares ETF").
    /// </summary>
    [Required]
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Exchange where this security trades (e.g., "ASX", "NASDAQ", "NYSE").
    /// </summary>
    [MaxLength(50)]
    public string? Exchange { get; set; }

    /// <summary>
    /// Type of security: "STOCK", "ETF", "CRYPTO", etc.
    /// </summary>
    // todo: enum?
    [Required]
    [MaxLength(20)]
    public string SecurityType { get; set; } = "STOCK";

    /// <summary>
    /// Trading currency (e.g., "AUD", "USD").
    /// </summary>
    [Required]
    [MaxLength(3)]
    public string Currency { get; set; } = "AUD";

    /// <summary>
    /// Industry sector (e.g., "Technology", "Healthcare").
    /// </summary>
    [MaxLength(100)]
    public string? Sector { get; set; }

    /// <summary>
    /// Specific industry (e.g., "Software", "Pharmaceuticals").
    /// </summary>
    [MaxLength(100)]
    public string? Industry { get; set; }

    /// <summary>
    /// When this security record was created.
    /// </summary>
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Last time this security record was updated.
    /// </summary>
    [Required]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// All holdings across all portfolios that reference this security.
    /// <remarks>todo: Not strictly required but if I ever want to do an admin dashboard, analytics etc., could be cyclic if not done properly</remarks>
    /// </summary>
    public virtual ICollection<Holding> Holdings { get; set; } = new List<Holding>();

    /// <summary>
    /// Historical price data for this security.
    /// </summary>
    public virtual ICollection<PriceHistory> PriceHistory { get; set; } = new List<PriceHistory>();
}