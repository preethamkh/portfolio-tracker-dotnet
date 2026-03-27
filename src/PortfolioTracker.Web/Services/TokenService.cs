using PortfolioTracker.Web.Interfaces.Services;

namespace PortfolioTracker.Web.Services;

public class TokenService(IHttpContextAccessor httpContextAccessor) : ITokenService
{
    private const string TokenKey = "jwt_token";

    public void SetToken(string token)
    {
        httpContextAccessor.HttpContext?.Session.SetString(TokenKey, token);
    }

    public string? GetToken()
    {
        return httpContextAccessor.HttpContext?.Session.GetString(TokenKey);
    }

    public void ClearToken()
    {
        httpContextAccessor.HttpContext?.Session.Remove(TokenKey);
    }
}
