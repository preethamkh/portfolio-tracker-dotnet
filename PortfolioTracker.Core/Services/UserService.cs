using PortfolioTracker.Core.DTOs.User;
using PortfolioTracker.Core.Interfaces.Services;

namespace PortfolioTracker.Core.Services
{
    /// <summary>
    /// Service implementation for user management.
    /// Contains all business logic related to users.
    /// </summary>
    /// <remarks>
    /// Service Layer Responsibilities:
    /// 1. Business logic validation
    /// 2. Entity to DTO mapping == (keeps controllers thin)
    /// 3. Calling repositories (when we add them)
    /// 4. Orchestrating multiple operations (i.e., updating related entities, calling multiple repositories, or handling transactions)
    /// 5. Logging business events (i.e., user created, user deleted)
    /// </remarks>
    public class UserService : IUserService
    {
        public Task<IEnumerable<UserDto>> GetAllUsersAsync()
        {
            throw new NotImplementedException();
        }

        public Task<UserDto?> GetUserByIdAsync(Guid id)
        {
            throw new NotImplementedException();
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
