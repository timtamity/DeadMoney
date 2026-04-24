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

            var discordId       = context.Principal.FindFirstValue(ClaimTypes.NameIdentifier);
            var discordUsername = context.Principal.Identity.Name ?? "Unknown User";
            var discordAvatar   = context.Principal.FindFirstValue("urn:discord:avatar:url");

            if (string.IsNullOrEmpty(discordId))
            {
                _logger.LogWarning("Discord login provided no NameIdentifier.");
                return;
            }

            using var db = await _dbFactory.CreateDbContextAsync();

            var user = await db.Users
                .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.DiscordId == discordId);

            if (user == null)
            {
                _logger.LogInformation("Provisioning new user: {Username} ({DiscordId})", discordUsername, discordId);
                user = new User
                {
                    DiscordId   = discordId,
                    Username    = discordUsername,
                    DisplayName = discordUsername,
                    AvatarUrl   = discordAvatar,
                    TimeZoneId  = "UTC",
                    CreatedAt   = DateTime.UtcNow,
                    LastLogin   = DateTime.UtcNow
                };
                db.Users.Add(user);
            }
            else
            {
                user.LastLogin = DateTime.UtcNow;
                user.Username  = discordUsername;
                if (!string.IsNullOrEmpty(discordAvatar)) user.AvatarUrl = discordAvatar;
            }

            await db.SaveChangesAsync();

            var appClaims = new List<Claim>
            {
                new("UserId",      user.Id.ToString()),
                new("DisplayName", user.DisplayName ?? user.Username),
                new("TimeZoneId",  user.TimeZoneId)
            };

            foreach (var ur in user.UserRoles ?? [])
            {
                if (ur.Role != null)
                    appClaims.Add(new Claim(ClaimTypes.Role, ur.Role.Name));
                if (ur.TeamId.HasValue)
                    appClaims.Add(new Claim("TeamId", ur.TeamId.Value.ToString()));
                if (ur.PositionId.HasValue && ur.Position != null)
                    appClaims.Add(new Claim("AgentPosition", ur.Position.Code));
            }

            context.Principal.AddIdentity(
                new ClaimsIdentity(appClaims, context.Principal.Identity.AuthenticationType));
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