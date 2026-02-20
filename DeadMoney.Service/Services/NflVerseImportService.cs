using CsvHelper;
using CsvHelper.Configuration;
using DeadMoney.Core.Entities;
using DeadMoney.Data;
using DeadMoney.Service.Interfaces;
using DuckDB.NET.Data;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.IO.Compression;
using System.Text;

namespace DeadMoney.Service.Services;

public class NflVerseImportService(
    DeadMoneyDbContext context,
    HttpClient http) : INflVerseImportService
{
    private const string PlayersUrl = "https://github.com/nflverse/nflverse-data/releases/download/players/players.csv";
    private const string RostersUrl = "https://github.com/nflverse/nflverse-data/releases/download/rosters/roster_2025.csv";
    private const string ContractsUrl = "https://github.com/nflverse/nflverse-data/releases/download/contracts/historical_contracts.parquet";

    public async Task<string> GetContractFileSchemaAsync()
    {
        try
        {
            using var connection = new DuckDBConnection("DataSource=:memory:");
            await connection.OpenAsync();
            using (var setupCmd = new DuckDBCommand("INSTALL httpfs; LOAD httpfs;", connection))
                await setupCmd.ExecuteNonQueryAsync();

            var query = $"DESCRIBE SELECT * FROM read_parquet('{ContractsUrl}');";
            using var command = new DuckDBCommand(query, connection);
            using var reader = await command.ExecuteReaderAsync();

            var sb = new StringBuilder();
            sb.AppendLine("--- DUCKDB PARQUET SCHEMA DUMP ---");
            while (reader.Read())
                sb.AppendLine($"Column: {reader[0],-20} | Type: {reader[1],-12}");

            return sb.ToString();
        }
        catch (Exception ex) { return $"Schema Dump Failed: {ex.Message}"; }
    }

    public async Task SyncPlayerMasterListAsync()
    {
        var records = await GetCsvRecordsAsync(PlayersUrl);

        // Load existing data into memory for fast lookup
        var existingPlayers = (await context.Players.Where(p => p.GsisId != null).ToListAsync())
            .GroupBy(p => p.GsisId!).ToDictionary(g => g.Key, g => g.First());

        var existingPositions = (await context.Positions.ToListAsync())
            .GroupBy(p => p.Code).ToDictionary(g => g.Key, g => g.First());

        foreach (var row in records)
        {
            IDictionary<string, object> dict = row;
            string gsisId = dict["gsis_id"]?.ToString() ?? "";
            string status = dict["status"]?.ToString()?.ToUpper() ?? "";

            if (string.IsNullOrEmpty(gsisId) || status == "RET") continue;

            // 1. Resolve the Position
            string posCode = MapPosition(dict["position"]?.ToString() ?? "UNK");

            if (!existingPositions.TryGetValue(posCode, out var position))
            {
                position = new Position { Code = posCode, Name = posCode };
                context.Positions.Add(position);

                // We must save here to generate the Integer ID for the new Position
                await context.SaveChangesAsync();
                existingPositions.Add(posCode, position);
            }

            // 2. Resolve the Player
            if (!existingPlayers.TryGetValue(gsisId, out Player? player))
            {
                player = new Player { GsisId = gsisId };
                context.Players.Add(player);
                existingPlayers.Add(gsisId, player);
            }

            // 3. Map Properties
            player.FirstName = dict["first_name"]?.ToString() ?? "";
            player.LastName = dict["last_name"]?.ToString() ?? "";
            player.OtcId = dict["otc_id"]?.ToString();

            // THE FIX: Assign the Integer ID from our resolved Position entity
            player.PositionId = position.Id;
        }

        await context.SaveChangesAsync();
    }

    public async Task SyncCurrentRostersAsync()
    {
        var records = await GetCsvRecordsAsync(RostersUrl);
        var teams = (await context.Teams.ToListAsync()).GroupBy(t => t.Abbreviation.ToUpper()).ToDictionary(g => g.Key, g => g.First());
        var players = (await context.Players.Where(p => p.GsisId != null).ToListAsync()).GroupBy(p => p.GsisId!).ToDictionary(g => g.Key, g => g.First());

        foreach (var row in records)
        {
            IDictionary<string, object> dict = row;
            if (players.TryGetValue(dict["gsis_id"]?.ToString() ?? "", out Player? player))
            {
                player.Number = dict["jersey_number"]?.ToString();
                if (teams.TryGetValue(dict["team"]?.ToString()?.ToUpper() ?? "", out Team? team))
                    player.TeamId = team.Id;
            }
        }
        await context.SaveChangesAsync();
    }

    public async Task SyncContractsAsync()
    {
        var tempCsvPath = Path.Combine(Path.GetTempPath(), "contracts_flattened.csv");
        try
        {
            using (var conn = new DuckDBConnection("DataSource=:memory:"))
            {
                await conn.OpenAsync();
                using (var setupCmd = new DuckDBCommand("INSTALL httpfs; LOAD httpfs;", conn))
                    await setupCmd.ExecuteNonQueryAsync();

                var exportQuery = $@"
                    COPY (
                        WITH flattened AS (
                            SELECT 
                                otc_id, 
                                (COALESCE(value, 0) * 1000000) as value, 
                                (COALESCE(guaranteed, 0) * 1000000) as guaranteed, 
                                year_signed, 
                                years, 
                                unnest(cols) as year_struct
                            FROM read_parquet('{ContractsUrl}')
                            WHERE is_active = true AND otc_id IS NOT NULL
                        )
                        SELECT 
                            otc_id, value, guaranteed, year_signed, years, 
                            year_struct.year as year_val, 
                            year_struct.team as team_nickname, 
                            (COALESCE(year_struct.base_salary, 0) * 1000000) as base_salary, 
                            (COALESCE(year_struct.prorated_bonus, 0) * 1000000) as prorated_bonus, 
                            (COALESCE(year_struct.option_bonus, 0) * 1000000) as option_bonus,
                            (COALESCE(year_struct.roster_bonus, 0) * 1000000) as roster_bonus,
                            (COALESCE(year_struct.workout_bonus, 0) * 1000000) as workout_bonus,
                            (COALESCE(year_struct.per_game_roster_bonus, 0) * 1000000) as per_game_bonus,
                            (COALESCE(year_struct.cap_number, 0) * 1000000) as cap_number
                        FROM flattened
                        WHERE year_struct.year != 'Total' 
                          AND TRY_CAST(year_struct.year AS INTEGER) >= year_signed
                    ) TO '{tempCsvPath}' (HEADER TRUE, DELIMITER ',');";

                using var cmd = new DuckDBCommand(exportQuery, conn);
                await cmd.ExecuteNonQueryAsync();
            }
            await ProcessFlattenedCsv(tempCsvPath);
        }
        finally
        {
            if (File.Exists(tempCsvPath)) File.Delete(tempCsvPath);
        }
    }

    private async Task ProcessFlattenedCsv(string csvPath)
    {
        await context.Database.ExecuteSqlRawAsync("DELETE FROM League.ContractYears");
        await context.Database.ExecuteSqlRawAsync("DELETE FROM League.Contracts");

        using var reader = new StreamReader(csvPath);
        using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HeaderValidated = null,
            MissingFieldFound = null
        });

        csv.Context.TypeConverterOptionsCache.GetOptions<decimal>().NullValues.Add("");
        csv.Context.TypeConverterOptionsCache.GetOptions<int>().NullValues.Add("");

        await csv.ReadAsync();
        csv.ReadHeader();

        var playerLookup = (await context.Players.Where(p => p.OtcId != null).ToListAsync())
            .GroupBy(p => p.OtcId!).ToDictionary(g => g.Key, g => g.First().Id);

        var teamNicknameLookup = (await context.Teams.ToListAsync())
            .ToDictionary(t => t.Nickname.ToUpper(), t => t.Id);

        var createdContracts = new Dictionary<string, int>();

        while (await csv.ReadAsync())
        {
            var otcId = csv.GetField<string>("otc_id") ?? string.Empty;
            if (string.IsNullOrEmpty(otcId) || !playerLookup.TryGetValue(otcId, out int pId)) continue;

            int yearSigned = csv.GetField<int>("year_signed");
            int contractDuration = csv.GetField<int>("years");

            if (!createdContracts.TryGetValue(otcId, out int contractId))
            {
                var contract = new Contract
                {
                    PlayerId = pId,
                    TotalValue = Math.Round(csv.GetField<decimal>("value"), 2),
                    TotalGuaranteed = Math.Round(csv.GetField<decimal>("guaranteed"), 2),
                    IsActive = true
                };
                context.Contracts.Add(contract);
                await context.SaveChangesAsync();
                contractId = contract.Id;
                createdContracts.Add(otcId, contractId);
            }

            int currentYear = csv.GetField<int>("year_val");
            bool isVoidYear = currentYear >= (yearSigned + contractDuration);

            string teamRawNickname = (csv.GetField<string>("team_nickname") ?? "").ToUpper();
            teamNicknameLookup.TryGetValue(teamRawNickname, out var teamId);

            context.ContractYears.Add(new ContractYear
            {
                ContractId = contractId,
                Year = currentYear,
                TeamId = teamId > 0 ? teamId : null,
                BaseSalary = isVoidYear ? 0 : Math.Round(csv.GetField<decimal>("base_salary"), 2),
                SigningBonusProration = Math.Round(csv.GetField<decimal>("prorated_bonus"), 2),
                OptionBonusProration = Math.Round(csv.GetField<decimal>("option_bonus"), 2),
                RosterBonus = Math.Round(csv.GetField<decimal>("roster_bonus"), 2),
                WorkoutBonus = Math.Round(csv.GetField<decimal>("workout_bonus"), 2),
                PerGameRosterBonus = Math.Round(csv.GetField<decimal>("per_game_bonus"), 2),
                CapNumber = Math.Round(csv.GetField<decimal>("cap_number"), 2),
                IsVoidYear = isVoidYear
            });
        }
        await context.SaveChangesAsync();
    }

    private async Task<IEnumerable<dynamic>> GetCsvRecordsAsync(string url)
    {
        var response = await http.GetAsync(url);
        var stream = await response.Content.ReadAsStreamAsync();
        if (url.EndsWith(".gz")) stream = new GZipStream(stream, CompressionMode.Decompress);
        using var reader = new StreamReader(stream);
        using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            PrepareHeaderForMatch = args => args.Header.ToLower(),
            MissingFieldFound = null,
            HeaderValidated = null
        });
        return csv.GetRecords<dynamic>().ToList();
    }

    private string MapPosition(string raw) => raw.ToUpper() switch
    {
        "SAF" or "FS" or "SS" => "S",
        "OG" or "LG" or "RG" => "G",
        "OT" or "LT" or "RT" => "T",
        _ => raw.ToUpper()
    };
}