using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

namespace DeadMoney.Service.Interfaces
{
    public interface IUserService
    {
        /// <summary>
        /// Intercepts the Discord login, matches it to a local user, 
        /// and injects custom roles into the auth cookie.
        /// </summary>
        Task ProcessDiscordLoginAsync(TicketReceivedContext context);

        /// <summary>
        /// Helper to quickly retrieve custom claims (like TeamId) from the current user.
        /// </summary>
        int? GetAssignedTeamId(ClaimsPrincipal user);
    }
}