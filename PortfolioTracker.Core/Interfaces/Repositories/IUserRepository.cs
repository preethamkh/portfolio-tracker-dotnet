using PortfolioTracker.Core.Entities;

namespace PortfolioTracker.Core.Interfaces.Repositories;

/// <summary>
/// Repository interface for User-specific data operations.
/// Extends generic repository and adds User-specific methods.
/// </summary>
public interface IUserRepository : IRepository<User>
{
    /// <summary>
    /// Get user by email address
    /// </summary>
    /// <param name="email"></param>
    /// <returns></returns>
    Task<User?> GetByEmailAsync(string email);

    /// <summary>
    /// Check if email is already taken by another user
    /// </summary>
    /// <param name="email"></param>
    /// <param name="excludeUserId"></param>
    /// <returns></returns>
    Task<bool> IsEmailTakenAsync(string email, Guid? excludeUserId = null);
}