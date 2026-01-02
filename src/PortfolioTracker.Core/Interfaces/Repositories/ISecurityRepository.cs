using PortfolioTracker.Core.Entities;

namespace PortfolioTracker.Core.Interfaces.Repositories;

/// <summary>
/// Repository interface for Security-specific database operations.
/// Securities are master data shared across all users.
/// </summary>
public interface ISecurityRepository : IRepository<Security>
{
    /// <summary>
    /// Gets a security by its trading symbol.
    /// </summary>
    /// <param name="symbol">Trading symbol (e.g., AAPL)</param>
    /// <returns>Security if found, null otherwise</returns>
    /// <remarks>
    /// Symbols are case-insensitive (AAPL == aapl).
    /// Used when:
    /// - User searches for a stock
    /// - Creating a holding (check if security exists)
    /// - Fetching price data for a symbol
    /// </remarks>
    Task<Security?> GetBySymbolAsync(string symbol);

    /// <summary>
    /// Searches for securities matching a query string.
    /// </summary>
    /// <param name="query">Search term (symbol or name)</param>
    /// <param name="limit">Maximum results to return</param>
    /// <returns>List of matching securities</returns>
    /// <remarks>
    /// Search behavior:
    /// - Searches both symbol and name
    /// - Case-insensitive
    /// - Partial matches (APP matches AAPL, APPN, etc.)
    /// - Prioritizes exact symbol matches
    /// 
    /// Example queries:
    /// - "APP" -> AAPL, APX, APPL
    /// - "Apple" -> AAPL (Apple Inc.)
    /// - "Vanguard" -> All Vanguard ETFs
    /// </remarks>
    Task<List<Security>> SearchAsync(string query, int limit = 10);


    /// <summary>
    /// Checks if a security with the given symbol already exists.
    /// </summary>
    /// <param name="symbol">Trading symbol</param>
    /// <returns>True if exists, false otherwise</returns>
    /// <remarks>
    /// Used before creating a new security to prevent duplicates.
    /// Symbols are unique in the database (unique index).
    /// </remarks>
    Task<bool> ExistsBySymbolAsync(string symbol);
}