using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using PortfolioTracker.Web.Interfaces.Services;
using PortfolioTracker.Web.Models.ViewModels.Auth;

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

    // POST: /Auth/Login
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _apiClient.LoginAsync(model);

        if (result == null || string.IsNullOrEmpty(result.Token))
        {
            ModelState.AddModelError(string.Empty, "Invalid email or password. Please try again.");
            return View(model);
        }

        _tokenService.SetToken(result.Token);

        // Sign in with cookie auth so [Authorize] works
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, result.Username),
            new(ClaimTypes.Email, result.Email),
        };

        var identity = new ClaimsIdentity(claims, "Cookies");
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync("Cookies", principal, new AuthenticationProperties
        {
            IsPersistent = model.RememberMe,
            ExpiresUtc = result.ExpiresAt
        });

        // Redirect to originally requested URL or dashboard
        if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            return Redirect(model.ReturnUrl);

        return RedirectToAction("Index", "Dashboard");
    }

    // GET /Auth/Register
    [HttpGet]
    public IActionResult Register()
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Dashboard");

        return View(new RegisterViewModel());
    }

    // POST /Auth/Register
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var result = await _apiClient.RegisterAsync(model);

        if (result == null || string.IsNullOrEmpty(result.Token))
        {
            ModelState.AddModelError(string.Empty, "Registration failed. The email may already be in use.");
            return View(model);
        }

        // Auto-login after registration
        _tokenService.SetToken(result.Token);

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, result.Username),
            new(ClaimTypes.Email, result.Email),
        };

        var identity = new ClaimsIdentity(claims, "Cookies");
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync("Cookies", principal, new AuthenticationProperties
        {
            IsPersistent = false,
            ExpiresUtc = result.ExpiresAt
        });

        return RedirectToAction("Index", "Dashboard");
    }

    // POST /Auth/Logout
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        _tokenService.ClearToken();
        await HttpContext.SignOutAsync("Cookies");
        return RedirectToAction("Login");
    }
}