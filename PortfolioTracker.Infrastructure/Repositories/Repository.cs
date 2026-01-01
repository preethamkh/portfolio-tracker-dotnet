using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using PortfolioTracker.Core.Interfaces.Repositories;
using PortfolioTracker.Infrastructure.Data;

namespace PortfolioTracker.Infrastructure.Repositories;

/// <summary>
/// Generic repository implementation using Entity Framework Core.
/// This lives in Infrastructure layer and can reference ApplicationDbContext.
/// </summary>
/// <remarks>
/// This is where the "stuff" happens:
/// - Core defines the interface (IRepository)
/// - Infrastructure implements it with EF Core
/// - Services depend on interface, not implementation
/// - We can swap EF Core for Dapper/MongoDB without changing services...
/// </remarks>
public class Repository<T> : IRepository<T> where T : class
{
    // Repository uses ApplicationDbContext directly - this is fine, as it's part of the infrastructure layer
    // Does not violate Dependency Inversion Principle because Core depends on IRepository<T> interface, not this implementation

    // If we had multiple database providers, then we would abstract DbContext behind another interface.
    // Or - if we are testing without any database
    // YAGNI concept applies here for now.
    protected readonly ApplicationDbContext Context;
    protected readonly DbSet<T> DbSet;

    public Repository(ApplicationDbContext context)
    {
        Context = context;
        DbSet = Context.Set<T>();
    }

    public virtual async Task<T?> GetByIdAsync(Guid id)
    {
        return await DbSet.FindAsync(id);
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        return await DbSet
            .AsNoTracking()
            .ToListAsync();
    }

    public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
    {
        return await DbSet
            .AsNoTracking()
            .Where(predicate)
            .ToListAsync();
    }

    public virtual async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
    {
        return await DbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(predicate);
    }

    public virtual async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate)
    {
        return await DbSet
            .AsNoTracking()
            .AnyAsync(predicate);   
    }

    public virtual async Task<T> AddAsync(T entity)
    {
        await DbSet.AddAsync(entity);
        return entity;
    }

    public virtual async Task UpdateAsync(T entity)
    {
        DbSet.Update(entity);
        // just to satisfy the async signature
        await Task.CompletedTask;
    }

    public virtual async Task DeleteAsync(T entity)
    {
        DbSet.Remove(entity);
        await Task.CompletedTask;
    }

    public async Task<int> SaveChangesAsync()
    {
        return await Context.SaveChangesAsync();
    }
}