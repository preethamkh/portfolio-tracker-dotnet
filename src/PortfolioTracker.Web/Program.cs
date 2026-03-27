using Microsoft.AspNetCore.Identity;
using PortfolioTracker.Web.Interfaces.Services;
using PortfolioTracker.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// -----------------------------------------------------------------------
// Services
// -----------------------------------------------------------------------

// Add services to the container.
builder.Services.AddControllersWithViews();

// Read the backend API base URL from config
var apiBaseUrl = builder.Configuration["ApiSettings:BaseUrl"] ?? throw new InvalidOperationException("ApiSettings:BaseUrl is not configured");

// Typed HttpClient - managed lifecycle, avoids socket exhaustion
builder.Services.AddHttpClient<IApiClient, ApiClient>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.Timeout = TimeSpan.FromSeconds(30);
});

// Token service - reads/writes JWT from server-side session
builder.Services.AddScoped<ITokenService, TokenService>();

// Session - stores the JWT token server-side
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(8);
    // avoids client side scripts accessing the session cookie
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.Cookie.Name = ".PortfolioTracker.Session";
});

// Cookie auth - lets [Authorize] attribute and User.Identity work in MVC
builder.Services.AddAuthentication("Cookies")
    .AddCookie("Cookies", options =>
    {
        options.LoginPath = "/Auth/Login";
        options.LogoutPath = "/Auth/Logout";
        options.AccessDeniedPath = "/Auth/Login";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.Cookie.Name = ".PortfolioTracker.Auth";
    });

builder.Services.AddAuthorization();

// IHttpContextAccessor - allows services to access HttpContext for session and auth info
builder.Services.AddHttpContextAccessor();

// -----------------------------------------------------------------------
// Middleware pipeline
// -----------------------------------------------------------------------

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

// serves wwwroot/css/output.css etc.
app.UseStaticFiles();

app.UseRouting();

// must come before UseAuthentication
app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

// Default route: / goes to Dashboard/Index
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}");

app.Run();
