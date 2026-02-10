using DeadMoney.Core.Entities;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;

namespace DeadMoney.Web.Identity;

internal sealed class IdentityUserAccessor(UserManager<ApplicationUser> userManager, NavigationManager navigationManager)
{
    public async Task<ApplicationUser> GetRequiredUserAsync(HttpContext context)
    {
        var user = await userManager.GetUserAsync(context.User);
        if (user is null)
        {
            navigationManager.NavigateTo("Account/InvalidUser");
        }
        return user!;
    }
}