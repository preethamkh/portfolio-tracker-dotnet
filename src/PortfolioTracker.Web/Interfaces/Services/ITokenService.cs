namespace PortfolioTracker.Web.Interfaces.Services;

public interface ITokenService
{
    void SetToken(string token);
    string? GetToken();
    void ClearToken();
}
