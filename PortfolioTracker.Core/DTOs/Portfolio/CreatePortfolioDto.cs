using System.ComponentModel.DataAnnotations;

namespace PortfolioTracker.Core.DTOs.Portfolio;

/// <summary>
/// Data Transfer Object for creating a new portfolio.
/// </summary>
public class CreatePortfolioDto
{
    /// <summary>
    /// Portfolio name (required, e.g., "Retirement Fund").
    /// </summary>
    [Required(ErrorMessage = "Portfolio name is required")]
    [MaxLength(255, ErrorMessage = "Portfolio name cannot exceed 255 characters")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Optional description of the portfolio's purpose or strategy.
    /// </summary>
    [MaxLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
    public string? Description { get; set; }

    /// <summary>
    /// Base currency for this portfolio (default: AUD).
    /// </summary>
    [MaxLength(3, ErrorMessage = "Currency code must be 3 characters")]
    [RegularExpression("^[A-Z]{3}$", ErrorMessage = "Currency must be a 3-letter code (e.g., AUD, USD)")]
    public string Currency { get; set; } = "AUD";

    /// <summary>
    /// Whether this should be the user's default portfolio.
    /// If true, any existing default will be changed to false.
    /// </summary>
    public bool IsDefault { get; set; } = false;
}