using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PortfolioTracker.Web.Controllers;

/// <summary>
/// Dashboard controller - landing page for authenticated users.
/// Requires user to be logged in via [Authorize] attribute.
/// </summary>
[Authorize]
public class DashboardController : Controller
{
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(ILogger<DashboardController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// GET /Dashboard or /Dashboard/Index
    /// Main dashboard landing page - shows user welcome and portfolio summary
    /// </summary>
    public IActionResult Index()
    {
        var username = User.Identity?.Name ?? "User";
        ViewData["Title"] = "Dashboard";
        return View();
    }
}
