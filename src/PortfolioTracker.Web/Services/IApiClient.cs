using PortfolioTracker.Web.Models.ViewModels.Auth;

namespace PortfolioTracker.Web.Services;

public interface IApiClient
{
    // Auth
    Task<AuthResponseDto?> LoginAsync();
}
