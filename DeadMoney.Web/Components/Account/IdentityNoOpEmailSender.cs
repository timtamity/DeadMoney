using DeadMoney.Core.Entities; // Ensure this is the ONLY ApplicationUser reference
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace DeadMoney.Web.Components.Account;

internal sealed class IdentityNoOpEmailSender : IEmailSender<ApplicationUser>
{
    // The implementation you shared earlier...
    public Task SendConfirmationLinkAsync(ApplicationUser user, string email, string confirmationLink) => Task.CompletedTask;
    public Task SendPasswordResetLinkAsync(ApplicationUser user, string email, string resetLink) => Task.CompletedTask;
    public Task SendPasswordResetCodeAsync(ApplicationUser user, string email, string resetCode) => Task.CompletedTask;
}