using System.ComponentModel.DataAnnotations;

namespace PortfolioTracker.Core.DTOs.Portfolio;

public class UpdatePortfolioDto
{
    /// <summary>
    /// Updated portfolio name (optional).
    /// </summary>
    [MaxLength(255, ErrorMessage = "Portfolio name cannot exceed 255 characters")]
    public string? Name { get; set; }

    /// <summary>
    /// Updated description (optional).
    /// </summary>
    [MaxLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
    public string? Description { get; set; }

    /// <summary>
    /// Updated currency (optional).
    /// </summary>
    [MaxLength(3, ErrorMessage = "Currency code must be 3 characters")]
    [RegularExpression("^[A-Z]{3}$", ErrorMessage = "Currency must be a 3-letter code (e.g., AUD, USD)")]
    public string? Currency { get; set; }

    /// <summary>
    /// Whether this should be the default portfolio (optional).
    /// </summary>
    public bool? IsDefault { get; set; }
}