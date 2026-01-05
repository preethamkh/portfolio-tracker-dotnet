using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PortfolioTracker.Core.Enums;

namespace PortfolioTracker.Core.Entities;

/// <summary>
/// Represents a buy or sell transaction for a holding.
/// Records the complete history of portfolio changes.
/// </summary>
public class Transaction
{
    /// <summary>
    /// Unique identifier for the transaction.
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    /// <summary>
    /// Foreign key to the holding this transaction belongs to.
    /// </summary>
    [Required]
    public Guid HoldingId { get; set; }

    /// <summary>
    /// Type of transaction: "BUY" or "SELL".
    /// </summary>
    [Required]
    [MaxLength(10)]
    public TransactionType TransactionType { get; set; }

    /// <summary>
    /// Number of shares bought or sold.
    /// Always positive (type indicates buy/sell).
    /// </summary>
    [Required]
    [Column(TypeName = "decimal(18,6)")]
    public decimal Shares { get; set; }

    /// <summary>
    /// Price per share at the time of transaction.
    /// </summary>
    [Required]
    [Column(TypeName = "decimal(18,4)")]
    public decimal PricePerShare { get; set; }

    /// <summary>
    /// Total transaction amount (Shares × PricePerShare).
    /// Calculated field, but stored for audit purposes.
    /// </summary>
    [Required]
    [Column(TypeName = "decimal(18,4)")]
    public decimal TotalAmount { get; set; }

    /// <summary>
    /// Brokerage fees, commissions, or other transaction costs.
    /// </summary>
    [Column(TypeName = "decimal(18,4)")]
    public decimal Fees { get; set; } = 0;

    /// <summary>
    /// Date when the transaction occurred.
    /// </summary>
    [Required]
    public DateTime TransactionDate { get; set; }

    /// <summary>
    /// Optional notes about the transaction.
    /// </summary>
    [MaxLength(500)]
    public string? Notes { get; set; }

    /// <summary>
    /// When this transaction record was created in the system.
    /// </summary>
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties

    /// <summary>
    /// The holding this transaction belongs to.
    /// </summary>
    [ForeignKey(nameof(HoldingId))]
    public virtual Holding Holding { get; set; } = null!;
}