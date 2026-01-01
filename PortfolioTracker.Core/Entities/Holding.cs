using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PortfolioTracker.Core.Entities;

/// <summary>
/// Represents a holding (position) within a portfolio
/// This is a specific stock, bond, ETF, crypto, or other asset owned in a portfolio
/// </summary>
public class Holding
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]   
    // todo: change all these Ids to make them more clean-code "readable" later?
    public Guid Id { get; set; }

    /// <summary>
    /// Foreign Key to the Portfolio this holding belongs to
    /// </summary>
    [Required]
    public Guid PortfolioId { get; set; }

    /// <summary>
    /// Foreign Key to the Security (asset) being held
    /// </summary>
    [Required]
    public Guid SecurityId { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,6)")]
    public decimal TotalShares { get; set; } = 0;

    /// <summary>
    /// Average cost per share (calculated from transactions)
    /// </summary>
    [Column(TypeName = "decimal(18,4)")]
    public decimal? AverageCost { get; set; }

    // todo: fees / brokerage field?

    [Required] 
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Required]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties

    [ForeignKey(nameof(PortfolioId))]
    public virtual Portfolio Portfolio { get; set; } = null!;

    [ForeignKey(nameof(SecurityId))]
    public virtual Security Security { get; set; } = null!;

    /// <summary>
    /// All buy/sell transactions associated with this holding
    /// </summary>
    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();

    /// <summary>
    /// Dividend payments received for this holding
    /// </summary>
    public virtual ICollection<Dividend> Dividends { get; set; } = new List<Dividend>();
}