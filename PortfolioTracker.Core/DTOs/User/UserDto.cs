namespace PortfolioTracker.Core.DTOs.User
{
    /// <summary>
    /// DTOs for User responses.
    /// This is what we send back to clients (never expose entity directly).
    /// </summary>
    /// <remarks>
    /// Use DTOs instead of entities
    /// 1. Security: Don't expose sensitive fields (PasswordHash)
    /// 2. API Stability: Can change database without breaking API
    /// 3. Flexibility: Can shape response differently than database
    /// 4. Performance: Only include needed fields
    /// </remarks>
    public class UserDto
    {
        /// <summary>
        /// User's unique identifier.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// User's email address.
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// User's full name.
        /// </summary>
        public string? FullName { get; set; }

        /// <summary>
        /// When the account was created.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Last successful login.
        /// </summary>
        public DateTime? LastLogin { get; set; }

        // Note: No PasswordHash! Never expose this!
        // Note: No Portfolios collection! We need to have separate endpoints for that, if a client needs a user's portfolio, it should call a dedicated endpoint for that data
    }
}
