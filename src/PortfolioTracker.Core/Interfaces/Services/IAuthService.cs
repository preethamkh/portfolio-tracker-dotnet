using PortfolioTracker.Core.DTOs.Auth;
using PortfolioTracker.Core.DTOs.Authentication;

namespace PortfolioTracker.Core.Interfaces.Services;

/// <summary>
/// Service interface for authentication operations.
/// </summary>
public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request);

    Task<AuthResponse> LoginAsync(LoginRequest request);
}
