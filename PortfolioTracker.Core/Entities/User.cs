using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PortfolioTracker.Core.Entities;

/// <summary>
/// Represents a user in the portfolio tracking system
/// Contains user account info.
/// </summary>
public class User
{
    /// <summary>
    /// Using guid for better security and harder to guess
    /// can generate client side if needed
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    [Required]
    [EmailAddress]
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    public string PasswordHash { get; set; } = string.Empty;

    [MaxLength(255)]
    public string? FullName { get; set; }

    // Use UTC to make sure it's consistent across time zones
    // No daylight savings issues
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Required]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Last login is null until first login, set later, hence nullable
    /// </summary>
    public DateTime? LastLogin { get; set; }

    /// <summary>
    /// Navigation property - One user can have multiple portfolios
    /// </summary>
    public virtual ICollection<Portfolio> Portfolios { get; set; } = new List<Portfolio>();
}