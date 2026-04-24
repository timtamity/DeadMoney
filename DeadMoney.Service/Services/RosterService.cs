using DeadMoney.Core.Entities;
using DeadMoney.Core.Enums;
using DeadMoney.Data;
using Microsoft.EntityFrameworkCore;

namespace DeadMoney.Service.Services;

public record ContractInput(int Years, decimal TotalValueM, decimal GuaranteedM, decimal SigningBonusM);

public class RosterService
{
    private readonly IDbContextFactory<DeadMoneyDbContext> _dbFactory;
    private readonly TransactionFeedService _feed;

    public RosterService(IDbContextFactory<DeadMoneyDbContext> dbFactory, TransactionFeedService feed)
    {
        _dbFactory = dbFactory;
        _feed      = feed;
    }

    public async Task CutPlayerAsync(int playerId, int teamId, int year, bool isPostJune1,
        int? performedByUserId = null, string? performedByUserName = null)
    {
        using var db = await _dbFactory.CreateDbContextAsync();

        var player = await db.Players
            .Include(p => p.Team)
            .Include(p => p.Contracts).ThenInclude(c => c.ContractYears)
            .FirstOrDefaultAsync(p => p.Id == playerId);

        if (player == null) return;

        var contract = player.Contracts.FirstOrDefault(c => c.IsActive);
        if (contract != null)
        {
            if (isPostJune1)
            {
                var currentYr = contract.ContractYears.FirstOrDefault(cy => cy.Year == year);
                if (currentYr != null)
                {
                    currentYr.BaseSalary        = 0;
                    currentYr.RosterBonus       = 0;
                    currentYr.WorkoutBonus      = 0;
                    currentYr.PerGameRosterBonus = 0;
                    currentYr.OtherBonus        = 0;
                    currentYr.GuaranteedAmount  = 0;
                    currentYr.CapNumber         = currentYr.SigningBonusProration + currentYr.OptionBonusProration;
                }

                var futureYears     = contract.ContractYears.Where(cy => cy.Year > year).ToList();
                var futureProration = futureYears.Sum(cy => cy.SigningBonusProration + cy.OptionBonusProration);

                foreach (var cy in futureYears)
                {
                    cy.TeamId               = null;
                    cy.BaseSalary           = 0;
                    cy.SigningBonusProration = 0;
                    cy.OptionBonusProration  = 0;
                    cy.RosterBonus          = 0;
                    cy.WorkoutBonus         = 0;
                    cy.OtherBonus           = 0;
                    cy.PerGameRosterBonus   = 0;
                    cy.GuaranteedAmount     = 0;
                    cy.CapNumber            = 0;
                    cy.IsVoidYear           = true;
                }

                if (futureProration > 0)
                {
                    var nextYear    = year + 1;
                    var nextYrEntry = contract.ContractYears.FirstOrDefault(cy => cy.Year == nextYear);
                    if (nextYrEntry != null)
                    {
                        nextYrEntry.IsVoidYear           = false;
                        nextYrEntry.TeamId               = teamId;
                        nextYrEntry.SigningBonusProration = futureProration;
                        nextYrEntry.CapNumber            = futureProration;
                    }
                    else
                    {
                        contract.ContractYears.Add(new ContractYear
                        {
                            Year                  = nextYear,
                            TeamId                = teamId,
                            SigningBonusProration  = futureProration,
                            CapNumber             = futureProration
                        });
                    }
                }
            }
            else
            {
                foreach (var cy in contract.ContractYears)
                {
                    if (cy.Year == year)
                    {
                        cy.BaseSalary        = 0;
                        cy.RosterBonus       = 0;
                        cy.WorkoutBonus      = 0;
                        cy.PerGameRosterBonus = 0;
                        cy.OtherBonus        = 0;
                        cy.GuaranteedAmount  = 0;
                        cy.CapNumber         = cy.SigningBonusProration + cy.OptionBonusProration;
                    }
                    else if (cy.Year > year)
                    {
                        cy.TeamId               = null;
                        cy.BaseSalary           = 0;
                        cy.SigningBonusProration = 0;
                        cy.OptionBonusProration  = 0;
                        cy.RosterBonus          = 0;
                        cy.WorkoutBonus         = 0;
                        cy.OtherBonus           = 0;
                        cy.PerGameRosterBonus   = 0;
                        cy.GuaranteedAmount     = 0;
                        cy.CapNumber            = 0;
                        cy.IsVoidYear           = true;
                    }
                }
            }

            contract.IsActive        = false;
            contract.IsModifiedBySim = true;
        }

        var teamAbbr   = player.Team?.Abbreviation;
        var playerName = player.FullName;
        player.TeamId  = null;

        db.Transactions.Add(new Transaction
        {
            Type                  = TransactionType.Cut,
            PlayerId              = player.Id,
            TeamId                = teamId,
            Details               = isPostJune1 ? "Post-June 1 cut" : "Standard cut",
            OccurredAt            = DateTime.UtcNow,
            PerformedByUserId     = performedByUserId,
            PerformedByUserName   = performedByUserName
        });

        await db.SaveChangesAsync();
        _feed.Notify(new TransactionDto(0, TransactionType.Cut, playerName, player.Id, null, teamAbbr, teamId, null, isPostJune1 ? "Post-June 1 cut" : "Standard cut", DateTime.UtcNow));
    }

    public async Task SignFreeAgentAsync(int playerId, int teamId, int year, ContractInput input,
        int? performedByUserId = null, string? performedByUserName = null)
    {
        using var db = await _dbFactory.CreateDbContextAsync();

        var player = await db.Players
            .Include(p => p.Contracts)
            .FirstOrDefaultAsync(p => p.Id == playerId);

        if (player == null || player.TeamId != null) return;

        var totalValue   = input.TotalValueM   * 1_000_000m;
        var guaranteed   = input.GuaranteedM   * 1_000_000m;
        var signingBonus = input.SigningBonusM * 1_000_000m;
        var sbProration  = input.Years > 0 ? signingBonus / input.Years : 0m;
        var basePerYear  = input.Years > 0 ? (totalValue - signingBonus) / input.Years : 0m;

        foreach (var c in player.Contracts.Where(c => c.IsActive))
            c.IsActive = false;

        var contract = new Contract
        {
            PlayerId        = player.Id,
            IsActive        = true,
            IsModifiedBySim = true,
            TotalValue      = totalValue,
            TotalGuaranteed = guaranteed,
            SigningBonus    = signingBonus
        };

        for (int i = 0; i < input.Years; i++)
        {
            contract.ContractYears.Add(new ContractYear
            {
                TeamId                = teamId,
                Year                  = year + i,
                BaseSalary            = basePerYear,
                SigningBonusProration = sbProration,
                GuaranteedAmount     = i == 0 ? guaranteed : 0m,
                CapNumber            = basePerYear + sbProration
            });
        }

        player.TeamId = teamId;
        db.Contracts.Add(contract);

        var team       = await db.Teams.FindAsync(teamId);
        var playerName = player.FullName;
        var details    = $"{input.Years}yr / ${input.TotalValueM:F1}M";

        db.Transactions.Add(new Transaction
        {
            Type                  = TransactionType.Signed,
            PlayerId              = player.Id,
            TeamId                = teamId,
            Details               = details,
            OccurredAt            = DateTime.UtcNow,
            PerformedByUserId     = performedByUserId,
            PerformedByUserName   = performedByUserName
        });

        await db.SaveChangesAsync();
        _feed.Notify(new TransactionDto(0, TransactionType.Signed, playerName, player.Id, null, team?.Abbreviation, teamId, null, details, DateTime.UtcNow));
    }

    public async Task ExtendPlayerAsync(int playerId, int teamId, int year, ContractInput input,
        int? performedByUserId = null, string? performedByUserName = null)
    {
        using var db = await _dbFactory.CreateDbContextAsync();

        var player = await db.Players
            .Include(p => p.Contracts).ThenInclude(c => c.ContractYears)
            .FirstOrDefaultAsync(p => p.Id == playerId);

        if (player == null) return;

        var oldContract = player.Contracts.FirstOrDefault(c => c.IsActive);
        if (oldContract != null)
        {
            oldContract.IsActive        = false;
            oldContract.IsModifiedBySim = true;
        }

        var totalValue   = input.TotalValueM   * 1_000_000m;
        var guaranteed   = input.GuaranteedM   * 1_000_000m;
        var signingBonus = input.SigningBonusM * 1_000_000m;
        var sbProration  = input.Years > 0 ? signingBonus / input.Years : 0m;
        var basePerYear  = input.Years > 0 ? (totalValue - signingBonus) / input.Years : 0m;

        var contract = new Contract
        {
            PlayerId        = player.Id,
            IsActive        = true,
            IsModifiedBySim = true,
            TotalValue      = totalValue,
            TotalGuaranteed = guaranteed,
            SigningBonus    = signingBonus
        };

        for (int i = 0; i < input.Years; i++)
        {
            contract.ContractYears.Add(new ContractYear
            {
                TeamId                = teamId,
                Year                  = year + i,
                BaseSalary            = basePerYear,
                SigningBonusProration = sbProration,
                GuaranteedAmount     = i == 0 ? guaranteed : 0m,
                CapNumber            = basePerYear + sbProration
            });
        }

        db.Contracts.Add(contract);

        var team       = await db.Teams.FindAsync(teamId);
        var playerName = player.FullName;
        var details    = $"{input.Years}yr / ${input.TotalValueM:F1}M";

        db.Transactions.Add(new Transaction
        {
            Type                  = TransactionType.Extended,
            PlayerId              = player.Id,
            TeamId                = teamId,
            Details               = details,
            OccurredAt            = DateTime.UtcNow,
            PerformedByUserId     = performedByUserId,
            PerformedByUserName   = performedByUserName
        });

        await db.SaveChangesAsync();
        _feed.Notify(new TransactionDto(0, TransactionType.Extended, playerName, player.Id, null, team?.Abbreviation, teamId, null, details, DateTime.UtcNow));
    }

    public async Task ExecuteTradeAsync(
        int teamAId, IEnumerable<int> teamAPlayerIds, IEnumerable<int> teamAPickIds,
        int teamBId, IEnumerable<int> teamBPlayerIds, IEnumerable<int> teamBPickIds,
        int year,
        int? performedByUserId = null, string? performedByUserName = null)
    {
        using var db = await _dbFactory.CreateDbContextAsync();

        var teamA = await db.Teams.FindAsync(teamAId);
        var teamB = await db.Teams.FindAsync(teamBId);

        var allPlayerIds = teamAPlayerIds.Concat(teamBPlayerIds).Distinct().ToList();
        var allPickIds   = teamAPickIds.Concat(teamBPickIds).Distinct().ToList();

        var players = await db.Players
            .Where(p => allPlayerIds.Contains(p.Id))
            .Include(p => p.Contracts).ThenInclude(c => c.ContractYears)
            .ToListAsync();

        var picks = allPickIds.Count > 0
            ? await db.DraftPicks.Where(dp => allPickIds.Contains(dp.Id)).ToListAsync()
            : new();

        var notifications = new List<TransactionDto>();

        foreach (var playerId in teamAPlayerIds)
        {
            var player = players.FirstOrDefault(p => p.Id == playerId);
            if (player == null || player.TeamId != teamAId) continue;
            MovePlayer(player, teamBId, year);
            db.Transactions.Add(new Transaction { Type = TransactionType.Traded, PlayerId = player.Id, TeamId = teamAId, ToTeamId = teamBId, OccurredAt = DateTime.UtcNow, PerformedByUserId = performedByUserId, PerformedByUserName = performedByUserName });
            notifications.Add(new TransactionDto(0, TransactionType.Traded, player.FullName, player.Id, null, teamA?.Abbreviation, teamAId, teamB?.Abbreviation, null, DateTime.UtcNow));
        }

        foreach (var playerId in teamBPlayerIds)
        {
            var player = players.FirstOrDefault(p => p.Id == playerId);
            if (player == null || player.TeamId != teamBId) continue;
            MovePlayer(player, teamAId, year);
            db.Transactions.Add(new Transaction { Type = TransactionType.Traded, PlayerId = player.Id, TeamId = teamBId, ToTeamId = teamAId, OccurredAt = DateTime.UtcNow, PerformedByUserId = performedByUserId, PerformedByUserName = performedByUserName });
            notifications.Add(new TransactionDto(0, TransactionType.Traded, player.FullName, player.Id, null, teamB?.Abbreviation, teamBId, teamA?.Abbreviation, null, DateTime.UtcNow));
        }

        foreach (var pickId in teamAPickIds)
        {
            var pick = picks.FirstOrDefault(dp => dp.Id == pickId);
            if (pick == null || pick.CurrentTeamId != teamAId) continue;
            pick.CurrentTeamId = teamBId;
            db.Transactions.Add(new Transaction { Type = TransactionType.Traded, DraftPickId = pick.Id, TeamId = teamAId, ToTeamId = teamBId, Details = pick.Label, OccurredAt = DateTime.UtcNow, PerformedByUserId = performedByUserId, PerformedByUserName = performedByUserName });
            notifications.Add(new TransactionDto(0, TransactionType.Traded, pick.Label, null, pick.Id, teamA?.Abbreviation, teamAId, teamB?.Abbreviation, pick.Label, DateTime.UtcNow));
        }

        foreach (var pickId in teamBPickIds)
        {
            var pick = picks.FirstOrDefault(dp => dp.Id == pickId);
            if (pick == null || pick.CurrentTeamId != teamBId) continue;
            pick.CurrentTeamId = teamAId;
            db.Transactions.Add(new Transaction { Type = TransactionType.Traded, DraftPickId = pick.Id, TeamId = teamBId, ToTeamId = teamAId, Details = pick.Label, OccurredAt = DateTime.UtcNow, PerformedByUserId = performedByUserId, PerformedByUserName = performedByUserName });
            notifications.Add(new TransactionDto(0, TransactionType.Traded, pick.Label, null, pick.Id, teamB?.Abbreviation, teamBId, teamA?.Abbreviation, pick.Label, DateTime.UtcNow));
        }

        await db.SaveChangesAsync();
        foreach (var n in notifications) _feed.Notify(n);
    }

    private static void MovePlayer(Player player, int toTeamId, int year)
    {
        player.TeamId = toTeamId;
        var active = player.Contracts.FirstOrDefault(c => c.IsActive);
        if (active == null) return;
        foreach (var cy in active.ContractYears.Where(cy => cy.Year >= year && !cy.IsVoidYear))
            cy.TeamId = toTeamId;
    }
}
