using System.Security.Claims;
using DeadMoney.Core.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DeadMoney.Web.Identity;

public class UserProvisioningService(UserManager<ApplicationUser> userManager, ILogger<UserProvisioningService> logger)
{
    public async Task ProvisionUserAsync(ClaimsPrincipal principal)
    {
        // NameIdentifier is the permanent, unique ID from Discord/Google
        var providerKey = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        var name = principal.FindFirstValue(ClaimTypes.Name) ?? "New User";

        if (string.IsNullOrEmpty(providerKey))
        {
            logger.LogWarning("External login provided no NameIdentifier. Cannot provision user.");
            return;
        }

        // Search for user by DiscordId/ProviderKey instead of Email
        var user = await userManager.Users
            .FirstOrDefaultAsync(u => u.DiscordId == providerKey);

        if (user == null)
        {
            logger.LogInformation("New user detected (ID: {ProviderKey}). Creating record.", providerKey);

            user = new ApplicationUser
            {
                // We use the unique provider key as the UserName to ensure no collisions
                // but keep the Discord 'Name' for display purposes if needed later.
                UserName = $"{name}_{providerKey.Substring(providerKey.Length - 4)}",
                DiscordId = providerKey,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(user);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                logger.LogError("Failed to provision user: {Errors}", errors);
                throw new Exception($"Failed to provision user: {errors}");
            }
        }
        else
        {
            logger.LogInformation("Existing user {UserName} logged in via external provider.", user.UserName);
        }
    }
}