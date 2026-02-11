using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

namespace DeadMoney.Service.Interfaces;

public interface IUserService
{
    /// <summary>
    /// Processes the Discord login, provisions/updates the user in the database,
    /// and enriches the ClaimsPrincipal with app-specific roles and data.
    /// </summary>
    Task ProcessDiscordLoginAsync(TicketReceivedContext context);

    /// <summary>
    /// Extracts the TeamId from the user's claims if they are a GM or Assistant GM.
    /// </summary>
    int? GetAssignedTeamId(ClaimsPrincipal user);

    /// <summary>
    /// Retrieves the user's preferred TimeZoneId from claims, defaulting to UTC.
    /// </summary>
    string GetUserTimeZone(ClaimsPrincipal user);

    /// <summary>
    /// Retrieves the list of Position codes (e.g., "QB", "WR") assigned to an Agent.
    /// </summary>
    IEnumerable<string> GetAgentPositions(ClaimsPrincipal user);
}