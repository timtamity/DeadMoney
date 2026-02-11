using CsvHelper;
using CsvHelper.Configuration;
using DeadMoney.Core.Entities;
using DeadMoney.Data;
using DeadMoney.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace DeadMoney.Services.Services;

public class NflVerseImportService(DeadMoneyDbContext context, HttpClient http) : INflVerseImportService
{
    private const string PlayersUrl = "https://github.com/nflverse/nflverse-data/releases/download/players/players.csv";
    private const string RostersUrl = "https://github.com/nflverse/nflverse-data/releases/download/rosters/roster.csv";
    private const string ContractsUrl = "https://github.com/nflverse/nflverse-data/releases/download/contracts/contracts.csv";

    public async Task SyncPlayerMasterListAsync()
    {
        var records = await GetCsvRecordsAsync(PlayersUrl);
        var existingPlayers = await context.Players.Where(p => p.GsisId != null).ToDictionaryAsync(p => p.GsisId!);
        var existingPositions = await context.Positions.ToDictionaryAsync(p => p.Code);

        foreach (var row in records)
        {
            IDictionary<string, object> dict = row;
            string gsisId = dict["gsis_id"]?.ToString() ?? "";
            if (string.IsNullOrEmpty(gsisId)) continue;

            string posCode = MapPosition(dict["position"]?.ToString() ?? "UNK");
            if (!existingPositions.ContainsKey(posCode))
            {
                var newPos = new Position { Code = posCode, Name = posCode };
                context.Positions.Add(newPos);
                existingPositions.Add(posCode, newPos);
                await context.SaveChangesAsync();
            }

            if (!existingPlayers.TryGetValue(gsisId, out Player? player))
            {
                player = new Player { GsisId = gsisId };
                context.Players.Add(player);
                existingPlayers.Add(gsisId, player);
            }

            player.FirstName = dict["first_name"]?.ToString() ?? "";
            player.LastName = dict["last_name"]?.ToString() ?? "";
            player.Suffix = dict["suffix"]?.ToString();
            player.OtcId = dict["otc_id"]?.ToString();
            player.PfrId = dict["pfr_id"]?.ToString();
            player.BirthDate = dict["birth_date"]?.ToString();
            player.College = dict["college_name"]?.ToString();
            player.PositionCode = posCode;
            player.Height = dict["height"]?.ToString();

            if (int.TryParse(dict["weight"]?.ToString(), out int w))
                player.Weight = w;
        }
        await context.SaveChangesAsync();
    }

    public async Task SyncCurrentRostersAsync()
    {
        var records = await GetCsvRecordsAsync(RostersUrl);
        var teams = await context.Teams.ToDictionaryAsync(t => t.Abbreviation);
        var players = await context.Players.Where(p => p.GsisId != null).ToDictionaryAsync(p => p.GsisId!);

        foreach (var row in records)
        {
            IDictionary<string, object> dict = row;
            string gsisId = dict["gsis_id"]?.ToString() ?? "";

            if (players.TryGetValue(gsisId, out Player? player))
            {
                player.Number = dict["jersey_number"]?.ToString();
                player.YearsExp = dict["years_exp"]?.ToString();
                player.HeadshotUrl = dict["headshot_url"]?.ToString();

                string teamAbbr = dict["team"]?.ToString() ?? "";
                if (!string.IsNullOrEmpty(teamAbbr))
                {
                    if (!teams.TryGetValue(teamAbbr, out Team? team))
                    {
                        team = new Team { Abbreviation = teamAbbr, City = "Unknown", Nickname = teamAbbr };
                        context.Teams.Add(team);
                        teams.Add(teamAbbr, team);
                        await context.SaveChangesAsync();
                    }
                    player.TeamId = team.Id;
                }
            }
        }
        await context.SaveChangesAsync();
    }

    public async Task SyncContractsAsync()
    {
        var records = await GetCsvRecordsAsync(ContractsUrl);

        // Use OTC ID for mapping as GSIS ID is sometimes missing in contract files
        var playersByOtc = await context.Players
            .Where(p => p.OtcId != null)
            .ToDictionaryAsync(p => p.OtcId!);

        foreach (var row in records)
        {
            IDictionary<string, object> dict = row;
            string otcId = dict["otc_id"]?.ToString() ?? "";

            if (string.IsNullOrEmpty(otcId) || !playersByOtc.TryGetValue(otcId, out Player? player))
                continue;

            // Check if this player already has a "Base" contract (not sim-modified)
            bool hasContract = await context.Contracts.AnyAsync(c => c.PlayerId == player.Id && !c.IsModifiedBySim);
            if (hasContract) continue;

            // Map standard OTC columns
            if (!decimal.TryParse(dict["value"]?.ToString(), out decimal totalValue)) continue;
            int.TryParse(dict["years"]?.ToString(), out int duration);
            decimal.TryParse(dict["guaranteed"]?.ToString(), out decimal guaranteed);

            var contract = new Contract
            {
                PlayerId = player.Id,
                IsActive = dict["is_active"]?.ToString()?.ToLower() == "true",
                IsModifiedBySim = false,
                SigningBonus = guaranteed // Simplified for baseline
            };

            context.Contracts.Add(contract);
            await context.SaveChangesAsync();

            // Create placeholder years based on contract length
            // NFLVerse's contracts.csv doesn't always provide the year-by-year split in one row.
            // We'll generate the years starting from the 'year_signed' column or current year.
            if (!int.TryParse(dict["year_signed"]?.ToString(), out int startYear))
                startYear = DateTime.UtcNow.Year;

            decimal avgSalary = duration > 0 ? totalValue / duration : totalValue;

            for (int i = 0; i < (duration > 0 ? duration : 1); i++)
            {
                context.ContractYears.Add(new ContractYear
                {
                    ContractId = contract.Id,
                    Year = startYear + i,
                    BaseSalary = avgSalary,
                    SigningBonusProration = guaranteed / (duration > 0 ? duration : 1)
                });
            }
        }
        await context.SaveChangesAsync();
    }

    private async Task<IEnumerable<dynamic>> GetCsvRecordsAsync(string url)
    {
        var response = await http.GetStreamAsync(url);
        using var reader = new StreamReader(response);
        using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            PrepareHeaderForMatch = args => args.Header.ToLower()
        });
        return csv.GetRecords<dynamic>().ToList();
    }

    private string MapPosition(string rawPos)
    {
        if (string.IsNullOrWhiteSpace(rawPos)) return "UNK";
        string p = rawPos.ToUpper().Trim();
        return p switch
        {
            "SAF" or "FS" or "SS" => "S",
            "OG" or "LG" or "RG" => "G",
            "OT" or "LT" or "RT" => "T",
            "ILB" or "OLB" or "MLB" => "LB",
            "ED" or "EDGE" => "DE",
            "NT" => "DT",
            _ => p
        };
    }
}