using System.Linq.Expressions;

namespace PortfolioTracker.Core.Interfaces.Repositories;

/// <summary>
/// Generic repository interface for data access operations.
/// This abstraction allows Core layer to define data operations
/// without depending on Infrastructure layer (ApplicationDbContext).
/// </summary>
/// <remarks>
/// Why Repository Pattern?
/// 1. Separation of Concerns: Business logic doesn't know about EF Core
/// 2. Testability: Easy to mock repositories in unit tests
/// 3. Flexibility: Can swap data access technology (EF → Dapper → MongoDB)
/// 4. Dependency Inversion: Core depends on interface, Infrastructure implements it
/// </remarks>
/// <typeparam name="T">Entity type</typeparam>
public interface IRepository<T> where T : class
{
    /// <summary>
    /// Get entity by ID
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task<T?> GetByIdAsync(Guid id);

    /// <summary>
    /// Get  all entities
    /// </summary>
    /// <returns></returns>
    Task<IEnumerable<T>> GetAllAsync();

    /// <summary>
    /// Find entities matching a condition
    /// </summary>
    /// <param name="predicate">Filter condition</param>
    /// <returns></returns>
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);

    /// <summary>
    /// Get first entity matching a condition or null
    /// </summary>
    /// <param name="predicate"></param>
    /// <returns></returns>
    Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);

    /// <summary>
    /// Check if any entity matches a condition
    /// </summary>
    /// <param name="predicate"></param>
    /// <returns></returns>
    Task<bool> AnyAsync(Expression<Func<T, bool>> predicate);

    /// <summary>
    /// Add a new entity
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    Task<T> AddAsync(T entity);

    /// <summary>
    /// Update an existing entity
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    Task UpdateAsync(T entity);

    /// <summary>
    /// Delete an entity
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    Task DeleteAsync(T entity);

    /// <summary>
    /// Save all changes
    /// </summary>
    /// <returns></returns>
    Task<int> SaveChangesAsync();
}