using BlazorFrontend;
using BlazorFrontend.Services;
using BlazorFrontend.Services.Interfaces;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// -----------------------------------------------------------------------
// Configuration
// -----------------------------------------------------------------------

// The API base URL comes from wwwroot/appsettings.json so it can be
// changed per environment (dev vs production) without a rebuild.
var apiBaseUrl = builder.Configuration["ApiBaseUrl"]
                 ?? throw new InvalidOperationException("ApiBaseUrl is not configured");

// -----------------------------------------------------------------------
// HTTP + Services (Clean Architecture: interface-first registrations)
// -----------------------------------------------------------------------

builder.Services.AddScoped(sp =>
{
    var client = new HttpClient { BaseAddress = new Uri(apiBaseUrl) };
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    return client;
});

// Token storage (localStorage) and raw auth endpoints
builder.Services.AddScoped<ILocalStorageService, LocalStorageService>();
builder.Services.AddScoped<ITokenProvider, TokenProvider>();
builder.Services.AddScoped<IApiClient, ApiClient>();
builder.Services.AddScoped<IAuthApi, AuthApi>();

// Auth: implements both IAuthService and AuthenticationStateProvider,
// registered as a single scoped instance so state stays consistent.
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<IAuthService>(sp => sp.GetRequiredService<AuthService>());
builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<AuthService>());

// Feature services
builder.Services.AddScoped<IPortfolioService, PortfolioService>();
builder.Services.AddScoped<IHoldingService, HoldingService>();
builder.Services.AddScoped<ITransactionService, TransactionService>();
builder.Services.AddScoped<ISecurityService, SecurityService>();

var host = builder.Build();

// Restore any persisted session BEFORE the router evaluates authorization.
// Without this, a refresh would briefly redirect authenticated users to login.
var authService = host.Services.GetRequiredService<IAuthService>();
await authService.InitializeAsync();

await host.RunAsync();
