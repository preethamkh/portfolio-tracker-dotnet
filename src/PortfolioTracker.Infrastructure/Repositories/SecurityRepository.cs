using Microsoft.EntityFrameworkCore;
using PortfolioTracker.Core.Entities;
using PortfolioTracker.Core.Interfaces.Repositories;
using PortfolioTracker.Infrastructure.Data;

namespace PortfolioTracker.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for Security entity.
/// Handles database operations for securities (stocks, ETFs, etc.).
/// </summary>
public class SecurityRepository : Repository<Security>, ISecurityRepository
{
    public SecurityRepository(ApplicationDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Gets a security by its trading symbol (case-insensitive).
    /// </summary>
    public async Task<Security?> GetBySymbolAsync(string symbol)
    {
        // todo: might not have to rewrite, but I believe if I were to use StringComparison it would not be translated to SQL properly
        return await DbSet.AsNoTracking().FirstOrDefaultAsync(s => s.Symbol.ToUpper() == symbol.ToUpper());
    }

    /// <summary>
    /// Searches securities by symbol or name.
    /// </summary>
    /// <remarks>
    /// Search Logic:
    /// 1. Exact symbol match (highest priority)
    /// 2. Symbol starts with query
    /// 3. Name contains query
    /// </remarks>
    public async Task<List<Security>> SearchAsync(string query, int limit = 10)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return new List<Security>();
        }

        var upperQuery = query.ToUpper();

        return await DbSet
            .AsNoTracking()
            .Where(s =>
                s.Symbol.ToUpper().Contains(upperQuery) ||
                s.Name.ToUpper().Contains(upperQuery))
            .OrderBy(s =>
                // Exact symbol match first
                s.Symbol.ToUpper() == upperQuery ? 0 :
                // Symbol starts with query
                s.Symbol.ToUpper().StartsWith(upperQuery) ? 1 :
                // Name contains query
                2)
            .ThenBy(s => s.Symbol) // Then alphabetically
            .Take(limit)
            .ToListAsync();
    }

    /// <summary>
    /// Checks if a security with the given symbol exists.
    /// </summary>
    public async Task<bool> ExistsBySymbolAsync(string symbol)
    {
       return await DbSet.AnyAsync(s => s.Symbol.ToUpper() == symbol.ToUpper());
    }
}
