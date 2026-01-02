using System.ComponentModel.DataAnnotations;

namespace PortfolioTracker.Core.DTOs.Security;

/// <summary>
/// DTO for updating security information.
/// Only master data fields that might change over time.
/// </summary>
/// <remarks>
/// Symbol could change, being de-listed etc.: something for later, not for MVP
/// </remarks>
public class UpdateSecurityDto
{
    [MaxLength(255)]
    public string? Name { get; set; }

    [MaxLength(50)]
    public string? Exchange { get; set; }

    [MaxLength(100)]
    public string? Sector { get; set; }

    [MaxLength(100)]
    public string? Industry { get; set; }
}
