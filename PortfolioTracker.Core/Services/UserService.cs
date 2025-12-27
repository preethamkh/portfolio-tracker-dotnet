using Microsoft.Extensions.Logging;
using PortfolioTracker.Core.DTOs.User;
using PortfolioTracker.Core.Interfaces.Repositories;
using PortfolioTracker.Core.Interfaces.Services;

namespace PortfolioTracker.Core.Services
{
    /// <summary>
    /// Service implementation for user management.
    /// NOW CORRECTLY depends only on interfaces (IUserRepository), not Infrastructure!
    /// </summary>
    /// <remarks>
    /// Service Layer Responsibilities:
    /// 1. Business logic validation
    /// 2. Entity to DTO mapping == (keeps controllers thin)
    /// 3. Calling repositories (when we add them)
    /// 4. Orchestrating multiple operations (i.e., updating related entities, calling multiple repositories, or handling transactions)
    /// 5. Logging business events (i.e., user created, user deleted)
    /// 
    /// CORRECT Architecture:
    /// UserService (Core) → IUserRepository (Core interface) ← UserRepository (Infrastructure implementation)
    /// 
    /// Core Layer Dependencies:
    /// Depends on Core.Interfaces (its own interfaces)
    /// Depends on Core.Entities (its own entities)
    /// Depends on Core.DTOs (its own DTOs)
    /// NEVER depends on Infrastructure
    /// NEVER depends on API
    /// </remarks>
    ///
    /// <summary>
    /// Constructor (converted to primary constructor) now injects IUserRepository interface, not ApplicationDbContext!
    /// </summary>
    public class UserService(IUserRepository userRepository, ILogger<UserService> logger)
        : IUserService
    {
        private readonly IUserRepository _userRepository = userRepository;
        private readonly ILogger<UserService> _logger = logger;

        /// <summary>
        /// Get all users
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
        {
            _logger.LogInformation("Retrieving all users");

            var users = await _userRepository.GetAllAsync();

            return users.Select(user => new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                CreatedAt = user.CreatedAt,
                LastLogin = user.LastLogin
            });
        }

        public async Task<UserDto?> GetUserByIdAsync(Guid id)
        {
            _logger.LogInformation("Retrieving user with ID: {UserId}", id);

            var user = await _userRepository.GetByIdAsync(id);

            if (user == null)
            {
                _logger.LogWarning("User with ID: {UserId} not found", id);
                return null;
            }

            _logger.LogInformation("Retrieved user: {Email}", user.Email);

            return new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                CreatedAt = user.CreatedAt,
                LastLogin = user.LastLogin
            };
        }

        public Task<UserDto?> GetUserByEmailAsync(string email)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteUserAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<bool> UserExistsAsync(string email)
        {
            throw new NotImplementedException();
        }
    }
}
