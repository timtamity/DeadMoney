using CsvHelper;
using CsvHelper.Configuration;
using DeadMoney.Core.Entities;
using DeadMoney.Data;
using DeadMoney.Service.Interfaces;
using DeadMoney.Service.Parsers;
using DuckDB.NET.Data;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.IO.Compression;
using System.Text;

namespace DeadMoney.Service.Services;

public class NflVerseImportService(
    DeadMoneyDbContext context,
    HttpClient http,
    NflVerseCsvParser parser) : INflVerseImportService
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
            {
                sb.AppendLine($"Column: {reader[0],-20} | Type: {reader[1],-12}");
            }
            return sb.ToString();
        }
        catch (Exception ex) { return $"Schema Dump Failed: {ex.Message}"; }
    }

    public async Task SyncPlayerMasterListAsync()
    {
        var records = await GetCsvRecordsAsync(PlayersUrl);

        // Safe lookup: Group by GsisId to handle duplicates in source CSV
        var existingPlayers = (await context.Players.Where(p => p.GsisId != null).ToListAsync())
            .GroupBy(p => p.GsisId!)
            .ToDictionary(g => g.Key, g => g.First());

        var existingPositions = (await context.Positions.ToListAsync())
            .GroupBy(p => p.Code)
            .ToDictionary(g => g.Key, g => g.First());

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
            player.OtcId = dict["otc_id"]?.ToString();
            player.PositionCode = posCode;
        }
        await context.SaveChangesAsync();
    }

    public async Task SyncCurrentRostersAsync()
    {
        var records = await GetCsvRecordsAsync(RostersUrl);

        var teams = (await context.Teams.ToListAsync())
            .GroupBy(t => t.Abbreviation.ToUpper())
            .ToDictionary(g => g.Key, g => g.First());

        var players = (await context.Players.Where(p => p.GsisId != null).ToListAsync())
            .GroupBy(p => p.GsisId!)
            .ToDictionary(g => g.Key, g => g.First());

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

                // Modified query to unnest while keeping the anchor year and length
                var exportQuery = $@"
                    COPY (
                        SELECT 
                            otc_id,
                            is_active,
                            value,
                            guaranteed,
                            year_signed,
                            years,
                            unnest(cols) as year_data
                        FROM read_parquet('{ContractsUrl}')
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
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

        await csv.ReadAsync();
        csv.ReadHeader();

        // Safe player lookup (handling duplicates in OtcId)
        var playerLookup = (await context.Players.Where(p => p.OtcId != null).ToListAsync())
            .GroupBy(p => p.OtcId!)
            .ToDictionary(g => g.Key, g => g.First().Id);

        // Safe team lookup (using Name from the year_data tuples)
        var teamLookup = (await context.Teams.ToListAsync())
            .GroupBy(t => t.Name.ToUpper())
            .ToDictionary(g => g.Key, g => g.First().Id);

        var createdContracts = new Dictionary<string, int>();

        while (await csv.ReadAsync())
        {
            var otcId = csv.GetField<string>("otc_id") ?? string.Empty;
            if (string.IsNullOrEmpty(otcId) || !playerLookup.TryGetValue(otcId, out int pId)) continue;

            // Anchor points for contract logic
            int yearSigned = csv.GetField<int>("year_signed");
            int contractDuration = csv.GetField<int>("years");

            if (!createdContracts.TryGetValue(otcId, out int contractId))
            {
                var contract = new Contract
                {
                    PlayerId = pId,
                    TotalValue = csv.GetField<decimal>("value"),
                    TotalGuaranteed = csv.GetField<decimal>("guaranteed"),
                    IsActive = csv.GetField<bool>("is_active")
                };
                context.Contracts.Add(contract);
                await context.SaveChangesAsync();
                contractId = contract.Id;
                createdContracts.Add(otcId, contractId);
            }

            if (csv.TryGetField<string>("year_data", out var yearDataRaw))
            {
                // Parser needs to populate NflVerseYearDto.TeamName
                var contractYears = parser.ParseNestedYears(yearDataRaw)
                    .Where(y => y.Year >= yearSigned)
                    .OrderBy(y => y.Year)
                    .ToList();

                for (int i = 0; i < contractYears.Count; i++)
                {
                    var yr = contractYears[i];
                    bool isVoidYear = (i >= contractDuration);

                    // Resolve team from the tuple data
                    int? teamId = null;
                    if (!string.IsNullOrEmpty(yr.TeamName) && teamLookup.TryGetValue(yr.TeamName.ToUpper(), out var tId))
                    {
                        teamId = tId;
                    }

                    context.ContractYears.Add(new ContractYear
                    {
                        ContractId = contractId,
                        Year = yr.Year,
                        TeamId = teamId,
                        BaseSalary = isVoidYear ? 0 : yr.BaseSalary,
                        SigningBonusProration = yr.SigningBonusProration,
                        CapNumber = yr.CapHit,
                        IsVoidYear = isVoidYear
                    });
                }
            }
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