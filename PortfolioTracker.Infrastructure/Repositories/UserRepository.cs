using Microsoft.EntityFrameworkCore;
using PortfolioTracker.Core.Entities;
using PortfolioTracker.Core.Interfaces.Repositories;
using PortfolioTracker.Infrastructure.Data;

namespace PortfolioTracker.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for User entity.
/// Implements User-specific data access operations.
/// </summary>
public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(ApplicationDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Get user by email address
    /// </summary>
    /// <param name="email"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public async Task<User?> GetByEmailAsync(string email)
    {
        return await DbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == email);
    }

    // exclude a specific user ID (for updates to their own user profile)
    public async Task<bool> IsEmailTakenAsync(string email, Guid? excludeUserId = null)
    {
        var query = DbSet.Where(u => u.Email == email);

        if (excludeUserId.HasValue)
        {
            query = query.Where(u => u.Id != excludeUserId.Value);
        }

        return await query.AnyAsync();                
    }
}