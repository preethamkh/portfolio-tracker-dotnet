using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace BlazorFrontend.Components.Shared;

/// <summary>
/// Redirects unauthenticated visitors to the login page, preserving the
/// originally requested URL so we can return there after sign-in.
/// </summary>
public partial class RedirectToLogin : ComponentBase
{
    [Inject] private NavigationManager Navigation { get; set; } = default!;

    protected override void OnInitialized()
    {
        var currentUrl = Navigation.Uri;
        Navigation.NavigateTo($"/login?returnUrl={Uri.EscapeDataString(currentUrl)}", forceLoad: true);
    }
}
