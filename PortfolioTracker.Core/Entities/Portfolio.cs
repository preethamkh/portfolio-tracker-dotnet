using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PortfolioTracker.Core.Entities;

/// <summary>
/// Represents an investment portfolio
/// A user can have multiple portfolios (ex: Retirement, Trading, Long-Term, Savings)
/// </summary>
public class Portfolio
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    /// <summary>
    /// Foreign Key to User record who owns the portfolio
    /// </summary>
    [Required]
    public Guid UserId { get; set; }

    /// <summary>
    /// Portfolio name (ex: Retirement, Trading, Long-Term, Savings)
    /// </summary>
    // todo: May be change this to enum later?
    [Required]
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Optional description of the portfolio's purpose
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Base currency for the portfolio
    /// </summary>
    // todo: May be change this to enum later?
    [Required]
    [MaxLength(3)]
    public string Currency { get; set; } = "AUD";

    /// <summary>
    /// User's default portfolio
    /// </summary>
    // todo: recheck if this is required
    public bool IsDefault { get; set; } = false;

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Required]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// The uer who owns the portfolio
    /// virtual - so that it enables lazy loading (i.e., autoload the related
    /// user entity from the db when we access the User property, if it hasn't been loaded)
    /// </summary>
    [ForeignKey(nameof(UserId))]
    public virtual User User { get; set; } = null!;

    /// <summary>
    /// Collection of holdings (positions) in the portfolio
    /// One portfolio can have multiple holdings
    /// </summary>
    public virtual ICollection<Holding> Holdings { get; set; } = new List<Holding>();

    /// <summary>
    /// Historical snapshots of the portfolio's value over time
    /// Used for performance tracking and reporting
    ///
    /// Why store snapshots?
    /// Best for: Fast queries, reliable historical reporting, and when we need to show charts or performance history quickly
    /// Downside: A bit more storage space
    /// vs. Computing on the fly: Less storage, but slower queries and complex calculations
    /// </summary>
    public virtual ICollection<PortfolioSnapshot> Snapshots { get; set; } = new List<PortfolioSnapshot>();
}