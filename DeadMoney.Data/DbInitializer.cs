using DeadMoney.Core.Entities;
using DeadMoney.Core.Enums;
using Microsoft.EntityFrameworkCore;

namespace DeadMoney.Data;

public static class DbInitializer
{
    /// <summary>
    /// STATIC SEEDING: Called in OnModelCreating. 
    /// Handles Roles, Positions, and Teams via Migrations.
    /// </summary>
    public static void SeedLeagueData(this ModelBuilder builder)
    {
        // 1. Roles (Auth Schema)
        builder.Entity<Role>().HasData(
            new Role { Id = 1, Name = "Commissioner", Description = "Full administrative access to league settings and users." },
            new Role { Id = 2, Name = "Agent", Description = "Manages specific player positions across the league." },
            new Role { Id = 3, Name = "GM", Description = "Manages a specific team's roster and salary cap." },
            new Role { Id = 4, Name = "Assistant GM", Description = "Assists in managing team operations." }
        );

        // 2. Positions (League Schema)
        builder.Entity<Position>().HasData(
            new Position { Id = 1, Code = "QB", Name = "Quarterback", DisplayOrder = 1, Unit = PositionUnit.Offense },
            new Position { Id = 2, Code = "RB", Name = "Running Back", DisplayOrder = 2, Unit = PositionUnit.Offense },
            new Position { Id = 3, Code = "FB", Name = "Fullback", DisplayOrder = 3, Unit = PositionUnit.Offense },
            new Position { Id = 4, Code = "WR", Name = "Wide Receiver", DisplayOrder = 4, Unit = PositionUnit.Offense },
            new Position { Id = 5, Code = "TE", Name = "Tight End", DisplayOrder = 5, Unit = PositionUnit.Offense },
            new Position { Id = 6, Code = "OT", Name = "Offensive Tackle", DisplayOrder = 6, Unit = PositionUnit.Offense },
            new Position { Id = 7, Code = "G", Name = "Offensive Guard", DisplayOrder = 7, Unit = PositionUnit.Offense },
            new Position { Id = 8, Code = "C", Name = "Center", DisplayOrder = 8, Unit = PositionUnit.Offense },
            new Position { Id = 9, Code = "EDGE", Name = "Edge Defender", DisplayOrder = 10, Unit = PositionUnit.Defense },
            new Position { Id = 10, Code = "DT", Name = "Interior Defensive Line", DisplayOrder = 11, Unit = PositionUnit.Defense },
            new Position { Id = 11, Code = "LB", Name = "Linebacker", DisplayOrder = 12, Unit = PositionUnit.Defense },
            new Position { Id = 12, Code = "CB", Name = "Cornerback", DisplayOrder = 13, Unit = PositionUnit.Defense },
            new Position { Id = 13, Code = "S", Name = "Safety", DisplayOrder = 14, Unit = PositionUnit.Defense },
            new Position { Id = 14, Code = "K", Name = "Kicker", DisplayOrder = 20, Unit = PositionUnit.SpecialTeams },
            new Position { Id = 15, Code = "P", Name = "Punter", DisplayOrder = 21, Unit = PositionUnit.SpecialTeams },
            new Position { Id = 16, Code = "LS", Name = "Long Snapper", DisplayOrder = 22, Unit = PositionUnit.SpecialTeams }
        );

        // 3. Teams (League Schema)
        builder.Entity<Team>().HasData(
            new Team { Id = 1, City = "Arizona", Nickname = "Cardinals", Abbreviation = "ARI", Conference = "NFC", Division = "West" },
            new Team { Id = 2, City = "Atlanta", Nickname = "Falcons", Abbreviation = "ATL", Conference = "NFC", Division = "South" },
            new Team { Id = 3, City = "Baltimore", Nickname = "Ravens", Abbreviation = "BAL", Conference = "AFC", Division = "North" },
            new Team { Id = 4, City = "Buffalo", Nickname = "Bills", Abbreviation = "BUF", Conference = "AFC", Division = "East" },
            new Team { Id = 5, City = "Carolina", Nickname = "Panthers", Abbreviation = "CAR", Conference = "NFC", Division = "South" },
            new Team { Id = 6, City = "Chicago", Nickname = "Bears", Abbreviation = "CHI", Conference = "NFC", Division = "North" },
            new Team { Id = 7, City = "Cincinnati", Nickname = "Bengals", Abbreviation = "CIN", Conference = "AFC", Division = "North" },
            new Team { Id = 8, City = "Cleveland", Nickname = "Browns", Abbreviation = "CLE", Conference = "AFC", Division = "North" },
            new Team { Id = 9, City = "Dallas", Nickname = "Cowboys", Abbreviation = "DAL", Conference = "NFC", Division = "East" },
            new Team { Id = 10, City = "Denver", Nickname = "Broncos", Abbreviation = "DEN", Conference = "AFC", Division = "West" },
            new Team { Id = 11, City = "Detroit", Nickname = "Lions", Abbreviation = "DET", Conference = "NFC", Division = "North" },
            new Team { Id = 12, City = "Green Bay", Nickname = "Packers", Abbreviation = "GB", Conference = "NFC", Division = "North" },
            new Team { Id = 13, City = "Houston", Nickname = "Texans", Abbreviation = "HOU", Conference = "AFC", Division = "South" },
            new Team { Id = 14, City = "Indianapolis", Nickname = "Colts", Abbreviation = "IND", Conference = "AFC", Division = "South" },
            new Team { Id = 15, City = "Jacksonville", Nickname = "Jaguars", Abbreviation = "JAX", Conference = "AFC", Division = "South" },
            new Team { Id = 16, City = "Kansas City", Nickname = "Chiefs", Abbreviation = "KC", Conference = "AFC", Division = "West" },
            new Team { Id = 17, City = "Las Vegas", Nickname = "Raiders", Abbreviation = "LV", Conference = "AFC", Division = "West" },
            new Team { Id = 18, City = "Los Angeles", Nickname = "Chargers", Abbreviation = "LAC", Conference = "AFC", Division = "West" },
            new Team { Id = 19, City = "Los Angeles", Nickname = "Rams", Abbreviation = "LAR", Conference = "NFC", Division = "West" },
            new Team { Id = 20, City = "Miami", Nickname = "Dolphins", Abbreviation = "MIA", Conference = "AFC", Division = "East" },
            new Team { Id = 21, City = "Minnesota", Nickname = "Vikings", Abbreviation = "MIN", Conference = "NFC", Division = "North" },
            new Team { Id = 22, City = "New England", Nickname = "Patriots", Abbreviation = "NE", Conference = "AFC", Division = "East" },
            new Team { Id = 23, City = "New Orleans", Nickname = "Saints", Abbreviation = "NO", Conference = "NFC", Division = "South" },
            new Team { Id = 24, City = "New York", Nickname = "Giants", Abbreviation = "NYG", Conference = "NFC", Division = "East" },
            new Team { Id = 25, City = "New York", Nickname = "Jets", Abbreviation = "NYJ", Conference = "AFC", Division = "East" },
            new Team { Id = 26, City = "Philadelphia", Nickname = "Eagles", Abbreviation = "PHI", Conference = "NFC", Division = "East" },
            new Team { Id = 27, City = "Pittsburgh", Nickname = "Steelers", Abbreviation = "PIT", Conference = "AFC", Division = "North" },
            new Team { Id = 28, City = "San Francisco", Nickname = "49ers", Abbreviation = "SF", Conference = "NFC", Division = "West" },
            new Team { Id = 29, City = "Seattle", Nickname = "Seahawks", Abbreviation = "SEA", Conference = "NFC", Division = "West" },
            new Team { Id = 30, City = "Tampa Bay", Nickname = "Buccaneers", Abbreviation = "TB", Conference = "NFC", Division = "South" },
            new Team { Id = 31, City = "Tennessee", Nickname = "Titans", Abbreviation = "TEN", Conference = "AFC", Division = "South" },
            new Team { Id = 32, City = "Washington", Nickname = "Commanders", Abbreviation = "WAS", Conference = "NFC", Division = "East" }
        );
    }

    /// <summary>
    /// RUNTIME SEEDING: Called in Program.cs at startup.
    /// Safely ensures your Discord user exists and is assigned to the Commissioner role.
    /// </summary>
    public static async Task InitializeUserRoles(DeadMoneyDbContext db, string discordId)
    {
        if (string.IsNullOrEmpty(discordId)) return;

        // 1. Find or create the User record
        var user = await db.Users
            .Include(u => u.UserRoles)
            .FirstOrDefaultAsync(u => u.DiscordId == discordId);

        if (user == null)
        {
            user = new User
            {
                DiscordId = discordId,
                Username = "tim_of_thee",
                DisplayName = "tim_of_thee",
                ThemePreference = "Dark",
                UseTeamColorsAsAccent = true,
                TimeZoneId = "UTC",
                CreatedAt = DateTime.UtcNow,
                LastLogin = DateTime.UtcNow
            };
            db.Users.Add(user);
            await db.SaveChangesAsync();
        }

        // 2. Assign the Commissioner Role (RoleId 1)
        // Now checking for existence based on the single PK structure 
        // with null Team/Position (League-wide admin)
        var hasCommRole = await db.UserRoles.AnyAsync(ur =>
            ur.UserId == user.Id &&
            ur.RoleId == 1 &&
            ur.PositionId == null &&
            ur.TeamId == null);

        if (!hasCommRole)
        {
            db.UserRoles.Add(new UserRole
            {
                UserId = user.Id,
                RoleId = 1,
                PositionId = null,
                TeamId = null,
                AssignedAt = DateTime.UtcNow
            });

            await db.SaveChangesAsync();
        }
    }
}