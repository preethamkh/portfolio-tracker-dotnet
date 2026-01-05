using Microsoft.EntityFrameworkCore;
using PortfolioTracker.Core.Entities;
using PortfolioTracker.Core.Interfaces.Repositories;
using PortfolioTracker.Infrastructure.Data;

namespace PortfolioTracker.Infrastructure.Repositories;

public class HoldingRepository : Repository<Holding>, IHoldingRepository
{
    // FYI - Weu need to pass the context to the base class so it can provide access to the correct DbSet<Holding> and manage all database operations. Without the context, the repository would not know how to access or track the entities.
    public HoldingRepository(ApplicationDbContext context) : base(context)
    {
    }

    // Using .Include() to eager load related Security entity for efficiency for some queries
    public async Task<IEnumerable<Holding>> GetByPortfolioIdAsync(Guid portfolioId)
    {
        return await DbSet
            .AsNoTracking()
            .Include(h => h.Security)
            .Where(h => h.PortfolioId == portfolioId)
            .OrderBy(h => h.Security
                .Symbol)
            .ToListAsync();
    }

    public async Task<Holding?> GetByIdWithDetailsAsync(Guid holdingId)
    {
        return await DbSet
            .AsNoTracking()
            .Include(h => h.Security)
            .Include(h => h.Portfolio)
            .FirstOrDefaultAsync(h => h.Id == holdingId);
    }

    public async Task<Holding?> GetByPortfolioAndSecurityAsync(Guid portfolioId, Guid securityId)
    {
        return await DbSet
            .AsNoTracking()
            .Include(h => h.Security)
            .FirstOrDefaultAsync(h => h.PortfolioId == portfolioId && h.SecurityId == securityId);
    }

    public async Task<bool> ExistsInPortfolioAsync(Guid holdingId, Guid portfolioId)
    {
        return await DbSet
            .AsNoTracking()
            .AnyAsync(h => h.Id == holdingId && h.PortfolioId == portfolioId);
    }

    public async Task<IEnumerable<Holding>> GetWithTransactionsAsync(Guid portfolioId)
    {
        return await DbSet
            .AsNoTracking()
            .Include(h => h.Security)
            .Include(h => h.Transactions)
            .OrderBy(h => h.Security.Symbol)
            .ToListAsync();
    }
}
