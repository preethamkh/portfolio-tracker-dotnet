using Microsoft.EntityFrameworkCore;
using PortfolioTracker.Core.Entities;
using PortfolioTracker.Core.Interfaces.Repositories;
using PortfolioTracker.Infrastructure.Data;

namespace PortfolioTracker.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for Portfolio entity.
/// Implements Portfolio-specific data access operations.
/// </summary>
public class PortfolioRepository : Repository<Portfolio>, IPortfolioRepository
{
    public PortfolioRepository(ApplicationDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Get all portfolios for a specific user.
    /// </summary>
    public async Task<IEnumerable<Portfolio>> GetByUserIdAsync(Guid userId)
    {
        return await DbSet
            .AsNoTracking()
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.IsDefault)    
            .ThenBy(p => p.Name)
            .ToListAsync();
    }

    /// <summary>
    /// Get a portfolio by ID, but only if it belongs to the specified user.
    /// This enforces authorization at the data layer.
    /// </summary>
    public async Task<Portfolio?> GetByIdAndUserIdAsync(Guid portfolioId, Guid userId)
    {
        return await DbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserId == userId && p.Id == portfolioId);
    }

    /// <summary>
    /// Get the user's default portfolio.
    /// </summary>
    public async Task<Portfolio?> GetDefaultPortfolioAsync(Guid userId)
    {
        return await DbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserId == userId && p.IsDefault);
    }

    /// <summary>
    /// Check if a user already has a portfolio with the given name.
    /// </summary>
    public async Task<bool> UserHasPortfolioWithNameAsync(Guid userId, string name, Guid? excludePortfolioId = null)
    {
        //return await DbSet
        //    .AsNoTracking()
        //    .AnyAsync(p => p.UserId == userId && p.Name == name && p.Id != excludePortfolioId);

        var query = DbSet.Where(p => p.UserId == userId && p.Name == name);

        if (excludePortfolioId.HasValue)
        {
            query = query.Where(p => p.Id != excludePortfolioId.Value);
        }

        return await query.AnyAsync();
    }

    /// <summary>
    /// Set a portfolio as the default for a user.
    /// This will unset any other default portfolio.
    /// </summary>
    public async Task SetAsDefaultAsync(Guid portfolioId, Guid userId)
    {
        // DB might not enforce single default portfolio per user,
        // so we need to unset any existing defaults first even though this looks weird.
        var currentDefaults = await DbSet
            .Where(p => p.UserId == userId && p.IsDefault)
            .ToListAsync();

        foreach (var portfolio in currentDefaults)
        {
            portfolio.IsDefault = false;
        }

        var newDefault = await DbSet.FindAsync(portfolioId);
        if (newDefault != null && newDefault.UserId == userId)
        {
            newDefault.IsDefault = true;
        }

        await Context.SaveChangesAsync();
    }

    /// <summary>
    /// Get portfolios with holdings count (for efficient list views).
    /// Uses GroupJoin to avoid N+1 queries.
    /// i.e., get all portfolios and their holdings count in one query.
    /// example: jin portfolios with their related holdings = count how many holdings each portfolio has in a single query -> this is more efficient than loading portfolios and then querying holdings for each one individually (N+1 problem)
    /// </summary>
    public async Task<IEnumerable<Portfolio>> GetWithHoldingsCountAsync(Guid userId)
    {
        // todo: to add this after Holdings entity is ready
        // for now, just return normal portfolios

        return await GetByUserIdAsync(userId);
    }
}