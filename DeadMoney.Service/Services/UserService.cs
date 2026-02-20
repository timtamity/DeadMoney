using DeadMoney.Core.Entities;
using DeadMoney.Data;
using DeadMoney.Service.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace DeadMoney.Service.Services
{
    public class UserService : IUserService
    {
        private readonly IDbContextFactory<DeadMoneyDbContext> _dbFactory;
        private readonly ILogger<UserService> _logger;

        public UserService(IDbContextFactory<DeadMoneyDbContext> dbFactory, ILogger<UserService> logger)
        {
            _dbFactory = dbFactory;
            _logger = logger;
        }

        public async Task ProcessDiscordLoginAsync(TicketReceivedContext context)
        {
            if (context.Principal?.Identity == null) return;

            // 1. Extract info from Discord
            var discordId = context.Principal.FindFirstValue(ClaimTypes.NameIdentifier);
            var discordUsername = context.Principal.Identity.Name ?? "Unknown User";
            var discordAvatar = context.Principal.FindFirstValue("urn:discord:avatar:url");

            if (string.IsNullOrEmpty(discordId))
            {
                _logger.LogWarning("Discord login provided no NameIdentifier.");
                return;
            }

            using var db = await _dbFactory.CreateDbContextAsync();

            // 2. Find or Create User - Including Positions for Agent logic
            var user = await db.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.DiscordId == discordId);

            if (user == null)
            {
                _logger.LogInformation("Provisioning new user: {Username} ({DiscordId})", discordUsername, discordId);
                user = new User
                {
                    DiscordId = discordId,
                    Username = discordUsername,
                    DisplayName = discordUsername,
                    AvatarUrl = discordAvatar,
                    TimeZoneId = "UTC",
                    CreatedAt = DateTime.UtcNow,
                    LastLogin = DateTime.UtcNow
                };
                db.Users.Add(user);
            }
            else
            {
                // Update existing user tracking
                user.LastLogin = DateTime.UtcNow;
                user.Username = discordUsername;
                if (!string.IsNullOrEmpty(discordAvatar)) user.AvatarUrl = discordAvatar;
            }

            await db.SaveChangesAsync();

            // 3. Bake the App-Specific Identity into the Cookie
            var appClaims = new List<Claim>
            {
                new Claim("UserId", user.Id.ToString()),
                new Claim("DisplayName", user.DisplayName ?? user.Username),
                new Claim("TimeZoneId", user.TimeZoneId)
            };

            // 4. Map Contextual Roles (Team & Position)
            if (user.UserRoles != null)
            {
                foreach (var userRole in user.UserRoles)
                {
                    // Add the basic Role Name (e.g., "Agent", "GM")
                    if (userRole.Role != null)
                    {
                        appClaims.Add(new Claim(ClaimTypes.Role, userRole.Role.Name));
                    }

                    // Add Team context if it exists
                    if (userRole.TeamId.HasValue)
                    {
                        appClaims.Add(new Claim("TeamId", userRole.TeamId.Value.ToString()));
                    }

                    // Add Position context if it exists (Replaces the nested loop)
                    if (userRole.PositionId.HasValue && userRole.Position != null)
                    {
                        appClaims.Add(new Claim("AgentPosition", userRole.Position.Code));
                    }
                }
            }

            var appIdentity = new ClaimsIdentity(appClaims, context.Principal.Identity.AuthenticationType);
            context.Principal.AddIdentity(appIdentity);
        }

        public int? GetAssignedTeamId(ClaimsPrincipal user)
        {
            var teamClaim = user.FindFirst("TeamId")?.Value;
            return int.TryParse(teamClaim, out var id) ? id : null;
        }

        public string GetUserTimeZone(ClaimsPrincipal user)
        {
            return user.FindFirst("TimeZoneId")?.Value ?? "UTC";
        }

        public IEnumerable<string> GetAgentPositions(ClaimsPrincipal user)
        {
            return user.FindAll("AgentPosition").Select(c => c.Value);
        }
    }
}