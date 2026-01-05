using Microsoft.EntityFrameworkCore;
using PortfolioTracker.Core.Entities;
using PortfolioTracker.Core.Interfaces.Repositories;
using PortfolioTracker.Infrastructure.Data;

namespace PortfolioTracker.Infrastructure.Repositories;

public class TransactionRepository : Repository<Transaction>, ITransactionRepository
{
    public TransactionRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Transaction>> GetByHoldingIdAsync(Guid holdingId)
    {
        return await DbSet
            .AsNoTracking()
            .Include(t => t.Holding)
            .ThenInclude(h => h.Security)
            .Where(t => t.HoldingId == holdingId)
            .OrderByDescending(t => t.TransactionDate)
            .ToListAsync();
    }

    public async Task<Transaction?> GetByIdWithDetailsAsync(Guid transactionId)
    {
        return await DbSet
            .AsNoTracking()
            .Include(t => t.Holding)
            .ThenInclude(h => h.Security)
            .FirstOrDefaultAsync(t => t.Id == transactionId);
    }

    public async Task<IEnumerable<Transaction>> GetByPortfolioIdAsync(Guid portfolioId)
    {
        return await DbSet
            .AsNoTracking()
            .Include(t => t.Holding)
            .ThenInclude(h => h.Security)
            .Where(t => t.Holding.PortfolioId == portfolioId)
            .OrderByDescending(t => t.TransactionDate)
            .ToListAsync();
    }

    public async Task<bool> ExistsInHoldingAsync(Guid transactionId, Guid holdingId)
    {
        return await DbSet
            .AsNoTracking()
            .AnyAsync(t => t.Id == transactionId && t.HoldingId == holdingId);
    }
}
