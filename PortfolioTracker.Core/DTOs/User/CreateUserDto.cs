using System.ComponentModel.DataAnnotations;

namespace PortfolioTracker.Core.DTOs.User;

/// <summary>
/// Data Transfer Object for creating a new user.
/// This defines what data is required from the client.
/// </summary>
public class CreateUserDto
{
    /// <summary>
    /// User's email address (required, must be valid email format).
    /// </summary>
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [MaxLength(255, ErrorMessage = "Email cannot exceed 255 characters")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// User's password (required, minimum 8 characters).
    /// </summary>
    [Required(ErrorMessage = "Password is required")]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters")]
    [MaxLength(100, ErrorMessage = "Password cannot exceed 100 characters")]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// User's full name (optional).
    /// </summary>
    [MaxLength(255, ErrorMessage = "Full name cannot exceed 255 characters")]
    public string? FullName { get; set; }
}