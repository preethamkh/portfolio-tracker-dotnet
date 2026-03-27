using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortfolioTracker.Web.Models.ViewModels.Transactions;
using PortfolioTracker.Web.Services;

namespace PortfolioTracker.Web.Controllers;

[Authorize]
public class TransactionsController : Controller
{
    private readonly IApiClient _apiClient;

    public TransactionsController(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    // GET /Transactions?holdingId=3
    public async Task<IActionResult> Index(int holdingId)
    {
        ViewData["Title"] = "Transactions";
        ViewData["HoldingId"] = holdingId;

        var holding = await _apiClient.GetHoldingAsync(holdingId);
        if (holding != null)
        {
            ViewData["Symbol"] = holding.Symbol;
            ViewData["PortfolioId"] = holding.PortfolioId;
        }

        var transactions = await _apiClient.GetTransactionsAsync(holdingId);
        return View(transactions);
    }

    // GET /Transactions/Create?holdingId=3
    public async Task<IActionResult> Create(int holdingId)
    {
        var holding = await _apiClient.GetHoldingAsync(holdingId);

        var model = new CreateTransactionViewModel
        {
            HoldingId = holdingId,
            Symbol = holding?.Symbol ?? string.Empty
        };

        ViewData["Title"] = $"Add Transaction — {model.Symbol}";
        return View(model);
    }

    // POST /Transactions/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateTransactionViewModel model)
    {
        if (!ModelState.IsValid)
        {
            ViewData["Title"] = $"Add Transaction — {model.Symbol}";
            return View(model);
        }

        var result = await _apiClient.CreateTransactionAsync(model);

        if (result == null)
        {
            ModelState.AddModelError(string.Empty, "Failed to record transaction.");
            ViewData["Title"] = $"Add Transaction — {model.Symbol}";
            return View(model);
        }

        TempData["Success"] = $"{model.TransactionType} transaction recorded for {model.Symbol}.";
        return RedirectToAction(nameof(Index), new { holdingId = model.HoldingId });
    }

    // POST /Transactions/Delete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, int holdingId)
    {
        var success = await _apiClient.DeleteTransactionAsync(id);

        TempData[success ? "Success" : "Error"] = success
            ? "Transaction deleted."
            : "Failed to delete transaction.";

        return RedirectToAction(nameof(Index), new { holdingId });
    }
}