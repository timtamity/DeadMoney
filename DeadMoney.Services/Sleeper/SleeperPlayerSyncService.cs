using DeadMoney.Core.Entities;
using DeadMoney.Core.Enums;
using DeadMoney.Data;
using DeadMoney.Services.Interfaces;
using DeadMoney.Services.Sleeper.Models;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Text.Json;

namespace DeadMoney.Services.Sleeper;

public class SleeperPlayerSyncService(DeadMoneyDbContext context, HttpClient http) : IPlayerSyncService
{
    private const string SleeperUrl = "https://api.sleeper.app/v1/players/nfl";

    private readonly Dictionary<string, string> _positionMap = new()
    {
        { "OG", "G" },
        { "OT", "T" },
        { "SAF", "S" },
        { "FS", "S" },
        { "SS", "S" }
    };

    public async Task SyncPlayersAsync()
    {
        var sw = Stopwatch.StartNew();
        context.Database.SetCommandTimeout(TimeSpan.FromMinutes(5));

        using var response = await http.GetAsync(SleeperUrl, HttpCompletionOption.ResponseHeadersRead);
        response.EnsureSuccessStatusCode();
        using var stream = await response.Content.ReadAsStreamAsync();

        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var sleeperPlayers = await JsonSerializer.DeserializeAsync<Dictionary<string, SleeperPlayerDto>>(stream, options);

        if (sleeperPlayers == null) return;

        var teams = await context.Teams.ToDictionaryAsync(t => t.Abbreviation);
        var positions = await context.Positions.ToDictionaryAsync(p => p.Code);
        var existingPlayers = await context.Players
            .Where(p => p.SleeperId != null)
            .ToDictionaryAsync(p => p.SleeperId!);

        foreach (var entry in sleeperPlayers)
        {
            var sleeperId = entry.Key;
            var dto = entry.Value;

            // 1. Skip if Names are "Player Invalid"
            if (dto.FirstName == "Player" && dto.LastName == "Invalid") continue;
            if (string.IsNullOrWhiteSpace(dto.FirstName) || string.IsNullOrWhiteSpace(dto.LastName)) continue;

            // 2. Ignore DEF and OTR positions entirely
            var rawPos = dto.Position ?? "OTR";
            if (rawPos == "DEF" || rawPos == "OTR") continue;

            // 3. Map Positions (OG -> G, etc.)
            var posCode = _positionMap.TryGetValue(rawPos, out var translated) ? translated : rawPos;

            // Ensure the position exists in the DB
            if (!positions.ContainsKey(posCode))
            {
                var newPos = new Position
                {
                    Code = posCode,
                    Name = posCode,
                    Unit = PositionUnit.Unknown,
                    DisplayOrder = 99
                };
                context.Positions.Add(newPos);
                positions.Add(posCode, newPos);
            }

            // 4. Update or Create Player
            // Inside your foreach loop in SyncPlayersAsync:
            if (!existingPlayers.TryGetValue(sleeperId, out var player))
            {
                player = new Player { SleeperId = sleeperId };
                context.Players.Add(player);
            }

            // Update all fields
            player.FirstName = dto.FirstName;
            player.LastName = dto.LastName;
            player.PositionCode = posCode;

            player.College = dto.College ?? "Unknown";
            player.Height = dto.Height ?? "N/A";
            player.Age = int.TryParse(dto.Age?.ToString(), out var age) ? age : (int?)null;
            player.Weight = int.TryParse(dto.Weight?.ToString(), out var weight) ? weight : (int?)null;
            player.Number = dto.Number?.ToString() ?? "00";

            // Experience: Convert "0" to "R", otherwise keep the value or default to "N/A"
            player.YearsExp = (dto.YearsExp?.ToString() == "0") ? "R" : (dto.YearsExp?.ToString() ?? "N/A");

            player.TeamId = (!string.IsNullOrEmpty(dto.Team) && teams.TryGetValue(dto.Team, out var team))
                ? team.Id
                : null;

            player.IsRetired = dto.Status?.ToLower() == "retired" || !dto.Active;
        }

        await context.SaveChangesAsync();
        sw.Stop();
        Debug.WriteLine($"Sync Successful: {existingPlayers.Count} players in {sw.Elapsed.TotalSeconds}s");
    }
}