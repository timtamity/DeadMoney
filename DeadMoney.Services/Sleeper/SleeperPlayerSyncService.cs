using DeadMoney.Core.Entities;
using DeadMoney.Data;
using DeadMoney.Services.Interfaces;
using DeadMoney.Services.Sleeper.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace DeadMoney.Services.Sleeper;

public class SleeperPlayerSyncService : IPlayerSyncService
{
    private readonly DeadMoneyDbContext _context;
    private readonly HttpClient _http;
    private const string SleeperUrl = "https://api.sleeper.app/v1/players/nfl";

    public SleeperPlayerSyncService(DeadMoneyDbContext context, HttpClient http)
    {
        _context = context;
        _http = http;
    }

    public async Task SyncPlayersAsync()
    {
        var response = await _http.GetStringAsync(SleeperUrl);
        var sleeperPlayers = JsonSerializer.Deserialize<Dictionary<string, SleeperPlayerDto>>(response);

        if (sleeperPlayers == null) return;

        var teams = await _context.Teams.ToDictionaryAsync(t => t.Abbreviation);
        var positions = await _context.Positions.ToDictionaryAsync(p => p.Code);

        // We'll use a combined key for uniqueness
        var existingPlayers = await _context.Players
            .ToDictionaryAsync(p => $"{p.FirstName}|{p.LastName}|{p.PositionCode}");

        foreach (var entry in sleeperPlayers.Values)
        {
            if (string.IsNullOrWhiteSpace(entry.FirstName) ||
                string.IsNullOrWhiteSpace(entry.LastName) ||
                string.IsNullOrWhiteSpace(entry.Position)) continue;

            // Only sync standard NFL positions that we have seeded
            if (!positions.ContainsKey(entry.Position)) continue;

            var key = $"{entry.FirstName}|{entry.LastName}|{entry.Position}";

            if (!existingPlayers.TryGetValue(key, out var player))
            {
                player = new Player
                {
                    FirstName = entry.FirstName,
                    LastName = entry.LastName,
                    PositionCode = entry.Position
                };
                _context.Players.Add(player);
                existingPlayers[key] = player; // Add to local dict to prevent duplicates in same run
            }

            // Update Team association
            player.TeamId = (!string.IsNullOrEmpty(entry.Team) && teams.TryGetValue(entry.Team, out var team))
                ? team.Id
                : null;

            player.IsRetired = entry.Status?.ToLower() == "retired";
        }

        await _context.SaveChangesAsync();
    }
}