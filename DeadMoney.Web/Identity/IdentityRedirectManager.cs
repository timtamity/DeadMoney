using Microsoft.AspNetCore.Components;

namespace DeadMoney.Web.Identity;

internal sealed class IdentityRedirectManager(NavigationManager navigationManager)
{
    public void RedirectTo(string? uri)
    {
        uri ??= "";

        if (!Uri.IsWellFormedUriString(uri, UriKind.Relative))
        {
            uri = navigationManager.ToBaseRelativePath(uri);
        }

        navigationManager.NavigateTo(uri);
    }

    public void RedirectToLogin() => RedirectTo("login");

    public void RedirectToWithStatus(string uri, string message, HttpContext context)
    {
        // Simple status passing via query string if needed later
        navigationManager.NavigateTo($"{uri}?status={Uri.EscapeDataString(message)}");
    }
}