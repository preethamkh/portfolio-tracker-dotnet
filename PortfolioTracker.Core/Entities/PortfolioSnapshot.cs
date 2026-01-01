using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PortfolioTracker.Core.Entities;

/// <summary>
/// Historical snapshots of the portfolio's value over time
/// Used for performance tracking and reporting
///
/// Why store snapshots?
/// Best for: Fast queries, reliable historical reporting, and when we need to show charts or performance history quickly
/// Downside: A bit more storage space
/// vs. Computing on the fly: Less storage, but slower queries and complex calculations
/// </summary>
public class PortfolioSnapshot
{
    [Required]
    public Guid Id { get; set; }

    /// <summary>
    /// Foreign key to the associated portfolio
    /// </summary>
    [Required]
    public Guid PortfolioId { get; set; }

    /// <summary>
    /// Total value of the portfolio at the time of the snapshot
    /// </summary>
    [Required]
    [Column(TypeName = "decimal(18, 4)")]
    public decimal TotalValue { get; set; }

    /// <summary>
    /// Total cost basis of the portfolio at the time of the snapshot
    /// (what was paid for all holdings)
    /// </summary>
    [Required]
    [Column(TypeName = "decimal(18, 4)")]
    public decimal TotalCost { get; set; }

    /// <summary>
    /// Total gain or loss (TotalValue - TotalCost)
    /// store in db for faster queries (performance), historical accuracy and / or to avoid calculation if expensive
    /// </summary>
    [Column(TypeName = "decimal(18, 4)")]
    public decimal? TotalGainLoss { get; set; }

    /// <summary>
    /// Date of this snapshot.
    /// One snapshot per portfolio per day.
    /// </summary>
    [Required]
    public DateTime SnapshotDate { get; set; }

    /// <summary>
    /// When did we actually save the snapshot?
    /// </summary>
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [Required]
    public virtual Portfolio Portfolio { get; set; } = null!;
}