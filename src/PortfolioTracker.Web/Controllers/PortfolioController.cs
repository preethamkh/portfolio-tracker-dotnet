using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PortfolioTracker.Web.Controllers;

/// <summary>
/// Portfolio controller - manages user portfolios.
/// Requires user to be logged in via [Authorize] attribute.
/// Used to test that unauthorized users get redirected to login.
/// </summary>
[Authorize]
public class PortfolioController : Controller
{
    private readonly ILogger<PortfolioController> _logger;

    public PortfolioController(ILogger<PortfolioController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// GET /Portfolio or /Portfolio/Index
    /// List all portfolios for the logged-in user.
    /// If not authenticated, should redirect to /Auth/Login
    /// </summary>
    public IActionResult Index()
    {
        ViewData["Title"] = "My Portfolios";
        return View();
    }
}
