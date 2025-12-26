using System.ComponentModel.DataAnnotations;

namespace PortfolioTracker.Core.DTOs.User
{
    /// <summary>
    /// DTO for updating user details.
    /// All fields are optional (partial update).
    /// </summary>
    public class UpdateUserDto
    {
        /// <summary>
        /// Updated email address (optional).
        /// </summary>
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [MaxLength(255, ErrorMessage = "Email cannot exceed 255 characters")]
        public string? Email { get; set; }

        /// <summary>
        /// Updated full name (optional).
        /// </summary>
        [MaxLength(255, ErrorMessage = "Full name cannot exceed 255 characters")]
        public string? FullName { get; set; }
    }
}
