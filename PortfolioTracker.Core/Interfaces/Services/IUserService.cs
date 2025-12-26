using PortfolioTracker.Core.DTOs.User;

namespace PortfolioTracker.Core.Interfaces.Services

{
    /// <summary>
    /// Service interface for user management operations.
    /// Defines the contract for business logic related to users.
    /// </summary>
    /// <remarks>
    /// Why use interfaces?
    /// 1. Dependency Inversion: Depend on abstractions, not concrete implementations
    /// 2. Testability: Easy to mock in unit tests
    /// 3. Flexibility: Can swap implementations without changing consumers
    /// 4. SOLID Principles: Interface Segregation Principle
    /// </remarks>
    public interface IUserService
    {
        /// <summary>
        /// Get all users.
        /// </summary>
        /// <returns>List of users</returns>
        Task<IEnumerable<UserDto>> GetAllUsersAsync();


        /// <summary>
        /// Get a user by their unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the user.</param>
        /// <returns>User DTO if found; otherwise, null.</returns>
        Task<UserDto?> GetUserByIdAsync(Guid id);

        /// <summary>
        /// Get a user by their email address.
        /// </summary>
        /// <param name="email">The email address of the user.</param>
        /// <returns>User DTO if found; otherwise, null.</returns>
        Task<UserDto?> GetUserByEmailAsync(string email);

        /// <summary>
        /// Delete a user by their unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the user.</param>
        /// <returns>True if the user was deleted; otherwise, false.</returns>
        Task<bool> DeleteUserAsync(Guid id);

        /// <summary>
        /// Check if user exists with given email.
        /// </summary>
        /// <param name="email">The email address to check.</param>
        /// <returns>True if the user exists; otherwise, false.</returns>
        Task<bool> UserExistsAsync(string email);
    }
}
