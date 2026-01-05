using PortfolioTracker.Core.DTOs.Security;

namespace PortfolioTracker.Core.Interfaces.Services;

/// <summary>
/// Service interface for Security operations.
/// </summary>
public interface ISecurityService
{
    Task<List<SecurityDto>> SearchSecuritiesAsync(string query, int limit = 10);
    Task<SecurityDto?> GetSecurityByIdAsync(Guid id);
    Task<SecurityDto?> GetSecurityBySymbolAsync(string symbol);
    Task<SecurityDto> GetOrCreateSecurityAsync(string symbol);
}
