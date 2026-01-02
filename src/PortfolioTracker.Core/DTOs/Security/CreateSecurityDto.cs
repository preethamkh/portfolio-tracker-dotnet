using System.ComponentModel.DataAnnotations;

namespace PortfolioTracker.Core.DTOs.Security;

/// <summary>
/// DTO for creating a new security.
/// Used when adding a security to the master data.
/// </summary>
/// <remarks>
/// Typically, securities are added when:
/// 1. User searches for a stock (we create it if not exists) -???
/// 2. Admin imports securities in bulk
/// 3. Background job fetches from external API
/// 
/// For MVP, we'll create securities on-demand when users add holdings. -??? or API?
/// </remarks>
public class CreateSecurityDto
{
    /// <summary>
    /// Trading symbol (must be unique)
    /// </summary>
    [Required(ErrorMessage = "Symbol is required")]
    [MaxLength(20)]
    public string Symbol { get; set; } = string.Empty;

    [Required(ErrorMessage = "Name is required")]
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? Exchange { get; set; }

    [Required]
    [MaxLength(20)]
    public string SecurityType { get; set; } = "STOCK";

    [Required]
    [MaxLength(3)]
    public string Currency { get; set; } = "AUD";

    [MaxLength(100)]
    public string? Sector { get; set; }

    [MaxLength(100)]
    public string? Industry { get; set; }
}
