using Microsoft.Extensions.Logging;
using PortfolioTracker.Core.DTOs.User;
using PortfolioTracker.Core.Entities;
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

            return users.Select(MapToUserDto);
        }
        
        /// <summary>
        /// Get a user by ID.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
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

            return MapToUserDto(user);
        }

        /// <summary>
        /// Get a user by email.
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<UserDto?> GetUserByEmailAsync(string email)
        {
            _logger.LogInformation("Retrieving user with email: {Email}", email);

            var user = await _userRepository.GetByEmailAsync(email);

            if (user == null)
            {
                _logger.LogWarning("User with email: {Email} not found", email);
                return null;
            }

            _logger.LogInformation("Retrieved user: {Email}", user.Email);

            return MapToUserDto(user);
        }

        public async Task<UserDto?> CreateUserAsync(CreateUserDto createUserDto)
        {
            _logger.LogInformation("Creating new user with email: {Email}", createUserDto.Email);

            // Business logic: Check if email already exists
            var emailTaken = await _userRepository.IsEmailTakenAsync(createUserDto.Email);

            if (emailTaken)
            {
                _logger.LogWarning("User with email {Email} already exists", createUserDto.Email);
                throw new InvalidOperationException($"User with email {createUserDto.Email} already exists");
            }

            // Map DTO to entity
            var user = new User
            {
                Email = createUserDto.Email,
                FullName = createUserDto.FullName,
                // todo: Hash password properly - this is TEMPORARY!
                PasswordHash = createUserDto.Password,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // save to db via repository
            await _userRepository.AddAsync(user);
            await _userRepository.SaveChangesAsync();

            _logger.LogInformation("Created user with ID: {UserId}", user.Id);

            // If you only returned a boolean or nothing, the client would have to make another call to fetch the user, which is less efficient, hence returning the created user DTO.
            return MapToUserDto(user);
        }

        /// <summary>
        /// Update user details.
        /// </summary>
        public async Task<UserDto?> UpdateUserAsync(Guid id, UpdateUserDto updateUserDto)
        {
            _logger.LogInformation("Updating user with ID: {UserId}", id);

            var user = await _userRepository.GetByIdAsync(id);

            if (user == null)
            {
                _logger.LogWarning("User with ID {UserId} not found", id);
                return null;
            }

            // Update email if provided
            if (!string.IsNullOrWhiteSpace(updateUserDto.Email) && updateUserDto.Email != user.Email)
            {
                // Business logic: Check if new email is already taken
                var emailTaken = await _userRepository.IsEmailTakenAsync(updateUserDto.Email, id);

                if (emailTaken)
                {
                    _logger.LogWarning("Email {Email} is already taken", updateUserDto.Email);
                    throw new InvalidOperationException($"Email {updateUserDto.Email} is already taken");
                }

                user.Email = updateUserDto.Email;
            }

            // Update full name if provided
            if (!string.IsNullOrWhiteSpace(updateUserDto.FullName))
            {
                user.FullName = updateUserDto.FullName;
            }

            // Save changes
            await _userRepository.UpdateAsync(user);
            await _userRepository.SaveChangesAsync();

            _logger.LogInformation("Updated user with ID: {UserId}", id);

            // If you only returned a boolean or nothing, the client would have to make another call to fetch the user, which is less efficient, hence returning the user DTO.
            return MapToUserDto(user);
        }

        /// <summary>
        /// Delete a user.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<bool> DeleteUserAsync(Guid id)
        {
            _logger.LogInformation("Deleting user with ID: {UserId}", id);

            var user = await _userRepository.GetByIdAsync(id);

            if (user == null)
            {
                _logger.LogWarning("User with ID {UserId} not found", id);
                return false;
            }

            await _userRepository.DeleteAsync(user);
            await _userRepository.SaveChangesAsync();

            _logger.LogInformation("Deleted user with ID: {UserId}", id);

            return true;
        }

        /// <summary>
        /// Check if a user exists with the given email.
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<bool> UserExistsAsync(string email)
        {
            return await _userRepository.IsEmailTakenAsync(email);
        }

        /// <summary>
        /// Maps a User entity to UserDto.
        /// </summary>
        private static UserDto MapToUserDto(User user)
        {
            return new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                CreatedAt = user.CreatedAt,
                LastLogin = user.LastLogin
            };
        }
    }
}
