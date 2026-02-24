using Microsoft.AspNetCore.Mvc;
using PortfolioTracker.Web.Interfaces.Services;

namespace PortfolioTracker.Web.Controllers;

public class AuthController : Controller
{
    private readonly IApiClient _apiClient;
    private readonly ITokenService _tokenService;

    public AuthController(IApiClient apiClient, ITokenService tokenService)
    {
        _apiClient = apiClient;
        _tokenService = tokenService;
    }

    // GET: /Auth/Login
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        // Already logged in? Redirect to dashboard
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Dashboard");
        }

        return View(new Models.ViewModels.Auth.LoginViewModel { ReturnUrl = returnUrl });
    }
}