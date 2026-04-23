using DeadMoney.Core.Entities;
using DeadMoney.Service.Services;
using DeadMoney.Tests.Helpers;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DeadMoney.Tests;

public class RosterServiceTests
{
    // Seeded team IDs from DbInitializer.SeedLeagueData (1 = ARI, 4 = BUF)
    private const int TeamId  = 1;
    private const int Team2Id = 4;
    private const int Year    = 2026;

    // ── Setup helpers ─────────────────────────────────────────────────────────

    private static TestDbContextFactory MakeFactory() => new();

    private static async Task<Player> SeedPlayerAsync(TestDbContextFactory factory, int? teamId = TeamId)
    {
        using var db = factory.CreateDbContext();
        var player = new Player { FirstName = "Test", LastName = "Player", PositionId = 1, TeamId = teamId };
        db.Players.Add(player);
        await db.SaveChangesAsync();
        return player;
    }

    private static async Task<Contract> SeedContractAsync(
        TestDbContextFactory factory,
        int playerId,
        int years,
        decimal capPerYear,
        decimal sbProrationPerYear)
    {
        using var db = factory.CreateDbContext();
        var contract = new Contract
        {
            PlayerId        = playerId,
            IsActive        = true,
            IsModifiedBySim = false,
            TotalValue      = capPerYear * years,
            SigningBonus    = sbProrationPerYear * years
        };
        for (int i = 0; i < years; i++)
        {
            contract.ContractYears.Add(new ContractYear
            {
                TeamId                = TeamId,
                Year                  = Year + i,
                BaseSalary            = capPerYear - sbProrationPerYear,
                SigningBonusProration = sbProrationPerYear,
                CapNumber             = capPerYear
            });
        }
        db.Contracts.Add(contract);
        await db.SaveChangesAsync();
        return contract;
    }

    // ── CutPlayerAsync — standard cut ─────────────────────────────────────────

    [Fact]
    public async Task CutPlayer_Standard_SetsPlayerTeamIdNull()
    {
        var factory = MakeFactory();
        var player  = await SeedPlayerAsync(factory);
        await SeedContractAsync(factory, player.Id, 3, 7_000_000m, 2_000_000m);
        var svc = new RosterService(factory);

        await svc.CutPlayerAsync(player.Id, TeamId, Year, isPostJune1: false);

        using var db = factory.CreateDbContext();
        var updated = await db.Players.FindAsync(player.Id);
        Assert.Null(updated!.TeamId);
    }

    [Fact]
    public async Task CutPlayer_Standard_SetsContractInactive()
    {
        var factory = MakeFactory();
        var player  = await SeedPlayerAsync(factory);
        var contract = await SeedContractAsync(factory, player.Id, 3, 7_000_000m, 2_000_000m);
        var svc = new RosterService(factory);

        await svc.CutPlayerAsync(player.Id, TeamId, Year, isPostJune1: false);

        using var db = factory.CreateDbContext();
        var updated = await db.Contracts.FindAsync(contract.Id);
        Assert.False(updated!.IsActive);
        Assert.True(updated.IsModifiedBySim);
    }

    [Fact]
    public async Task CutPlayer_Standard_CurrentYearCapNumberEqualsProrationOnly()
    {
        var factory = MakeFactory();
        var player  = await SeedPlayerAsync(factory);
        var contract = await SeedContractAsync(factory, player.Id, 3, 7_000_000m, 2_000_000m);
        var svc = new RosterService(factory);

        await svc.CutPlayerAsync(player.Id, TeamId, Year, isPostJune1: false);

        using var db = factory.CreateDbContext();
        var currentYr = await db.ContractYears
            .FirstAsync(cy => cy.ContractId == contract.Id && cy.Year == Year);

        Assert.Equal(2_000_000m, currentYr.CapNumber);
        Assert.Equal(0m, currentYr.BaseSalary);
    }

    [Fact]
    public async Task CutPlayer_Standard_FutureYearsAreVoidedAndTeamCleared()
    {
        var factory = MakeFactory();
        var player  = await SeedPlayerAsync(factory);
        var contract = await SeedContractAsync(factory, player.Id, 3, 7_000_000m, 2_000_000m);
        var svc = new RosterService(factory);

        await svc.CutPlayerAsync(player.Id, TeamId, Year, isPostJune1: false);

        using var db = factory.CreateDbContext();
        var futureYears = await db.ContractYears
            .Where(cy => cy.ContractId == contract.Id && cy.Year > Year)
            .ToListAsync();

        Assert.All(futureYears, cy =>
        {
            Assert.True(cy.IsVoidYear);
            Assert.Null(cy.TeamId);
            Assert.Equal(0m, cy.CapNumber);
        });
    }

    // ── CutPlayerAsync — post-June 1 cut ─────────────────────────────────────

    [Fact]
    public async Task CutPlayer_PostJune1_CurrentYearCapNumberIsCurrentProrationOnly()
    {
        var factory = MakeFactory();
        var player  = await SeedPlayerAsync(factory);
        var contract = await SeedContractAsync(factory, player.Id, 3, 7_000_000m, 2_000_000m);
        var svc = new RosterService(factory);

        await svc.CutPlayerAsync(player.Id, TeamId, Year, isPostJune1: true);

        using var db = factory.CreateDbContext();
        var currentYr = await db.ContractYears
            .FirstAsync(cy => cy.ContractId == contract.Id && cy.Year == Year);

        // Only the current year's own proration remains
        Assert.Equal(2_000_000m, currentYr.CapNumber);
    }

    [Fact]
    public async Task CutPlayer_PostJune1_FutureProrationAppearsInNextYear()
    {
        var factory = MakeFactory();
        var player  = await SeedPlayerAsync(factory);
        var contract = await SeedContractAsync(factory, player.Id, 3, 7_000_000m, 2_000_000m);
        var svc = new RosterService(factory);

        await svc.CutPlayerAsync(player.Id, TeamId, Year, isPostJune1: true);

        using var db = factory.CreateDbContext();
        // Future proration = years 2027 + 2028 × $2M each = $4M — should land on Year+1
        var nextYrEntry = await db.ContractYears
            .FirstOrDefaultAsync(cy => cy.ContractId == contract.Id && cy.Year == Year + 1 && !cy.IsVoidYear);

        Assert.NotNull(nextYrEntry);
        Assert.Equal(4_000_000m, nextYrEntry!.CapNumber);
        Assert.Equal(TeamId, nextYrEntry.TeamId);
    }

    [Fact]
    public async Task CutPlayer_PostJune1_NoFutureProration_NoNextYearEntryCreated()
    {
        var factory = MakeFactory();
        var player  = await SeedPlayerAsync(factory);
        // Single year contract — no future proration to defer
        var contract = await SeedContractAsync(factory, player.Id, 1, 5_000_000m, 2_000_000m);
        var svc = new RosterService(factory);

        await svc.CutPlayerAsync(player.Id, TeamId, Year, isPostJune1: true);

        using var db = factory.CreateDbContext();
        var nextYrCount = await db.ContractYears
            .CountAsync(cy => cy.ContractId == contract.Id && cy.Year == Year + 1);

        Assert.Equal(0, nextYrCount);
    }

    // ── SignFreeAgentAsync ────────────────────────────────────────────────────

    [Fact]
    public async Task SignFreeAgent_CreatesActiveContract()
    {
        var factory = MakeFactory();
        var player  = await SeedPlayerAsync(factory, teamId: null); // free agent
        var svc = new RosterService(factory);

        await svc.SignFreeAgentAsync(player.Id, TeamId, Year,
            new ContractInput(Years: 3, TotalValueM: 30m, GuaranteedM: 15m, SigningBonusM: 6m));

        using var db = factory.CreateDbContext();
        var contract = await db.Contracts
            .Include(c => c.ContractYears)
            .FirstOrDefaultAsync(c => c.PlayerId == player.Id && c.IsActive);

        Assert.NotNull(contract);
        Assert.True(contract!.IsModifiedBySim);
        Assert.Equal(30_000_000m, contract.TotalValue);
        Assert.Equal(15_000_000m, contract.TotalGuaranteed);
        Assert.Equal(6_000_000m,  contract.SigningBonus);
    }

    [Fact]
    public async Task SignFreeAgent_CreatesCorrectNumberOfContractYears()
    {
        var factory = MakeFactory();
        var player  = await SeedPlayerAsync(factory, teamId: null);
        var svc = new RosterService(factory);

        await svc.SignFreeAgentAsync(player.Id, TeamId, Year,
            new ContractInput(Years: 4, TotalValueM: 40m, GuaranteedM: 20m, SigningBonusM: 8m));

        using var db = factory.CreateDbContext();
        var count = await db.ContractYears
            .CountAsync(cy => cy.TeamId == TeamId && cy.Year >= Year && cy.Year < Year + 4);

        Assert.Equal(4, count);
    }

    [Fact]
    public async Task SignFreeAgent_SetsPlayerTeamId()
    {
        var factory = MakeFactory();
        var player  = await SeedPlayerAsync(factory, teamId: null);
        var svc = new RosterService(factory);

        await svc.SignFreeAgentAsync(player.Id, TeamId, Year,
            new ContractInput(Years: 2, TotalValueM: 20m, GuaranteedM: 10m, SigningBonusM: 4m));

        using var db = factory.CreateDbContext();
        var updated = await db.Players.FindAsync(player.Id);
        Assert.Equal(TeamId, updated!.TeamId);
    }

    [Fact]
    public async Task SignFreeAgent_CapNumberEqualsBaseAndProrationSumPerYear()
    {
        var factory = MakeFactory();
        var player  = await SeedPlayerAsync(factory, teamId: null);
        var svc = new RosterService(factory);

        // $30M total, $6M signing bonus → $24M base over 3 years = $8M/yr base, $2M/yr proration
        await svc.SignFreeAgentAsync(player.Id, TeamId, Year,
            new ContractInput(Years: 3, TotalValueM: 30m, GuaranteedM: 15m, SigningBonusM: 6m));

        using var db = factory.CreateDbContext();
        var years = await db.ContractYears
            .Where(cy => cy.TeamId == TeamId && cy.Year >= Year)
            .ToListAsync();

        Assert.All(years, cy =>
        {
            Assert.Equal(cy.BaseSalary + cy.SigningBonusProration, cy.CapNumber);
        });
    }

    [Fact]
    public async Task SignFreeAgent_PlayerAlreadyOnTeam_DoesNothing()
    {
        var factory  = MakeFactory();
        var player   = await SeedPlayerAsync(factory, teamId: TeamId); // already on a team
        var svc = new RosterService(factory);

        await svc.SignFreeAgentAsync(player.Id, Team2Id, Year,
            new ContractInput(Years: 2, TotalValueM: 20m, GuaranteedM: 10m, SigningBonusM: 4m));

        using var db = factory.CreateDbContext();
        var contractCount = await db.Contracts.CountAsync(c => c.PlayerId == player.Id);
        Assert.Equal(0, contractCount); // no contract created
    }

    [Fact]
    public async Task SignFreeAgent_DeactivatesExistingActiveContracts()
    {
        var factory = MakeFactory();
        var player  = await SeedPlayerAsync(factory, teamId: null);
        // Seed an old active contract (e.g., leftover data)
        await SeedContractAsync(factory, player.Id, 1, 3_000_000m, 500_000m);
        var svc = new RosterService(factory);

        await svc.SignFreeAgentAsync(player.Id, TeamId, Year,
            new ContractInput(Years: 2, TotalValueM: 20m, GuaranteedM: 10m, SigningBonusM: 4m));

        using var db = factory.CreateDbContext();
        var activeContracts = await db.Contracts
            .Where(c => c.PlayerId == player.Id && c.IsActive)
            .ToListAsync();

        Assert.Single(activeContracts); // only the new one
    }

    // ── ExtendPlayerAsync ─────────────────────────────────────────────────────

    [Fact]
    public async Task ExtendPlayer_DeactivatesOldContract()
    {
        var factory  = MakeFactory();
        var player   = await SeedPlayerAsync(factory);
        var oldContract = await SeedContractAsync(factory, player.Id, 2, 8_000_000m, 2_000_000m);
        var svc = new RosterService(factory);

        await svc.ExtendPlayerAsync(player.Id, TeamId, Year,
            new ContractInput(Years: 3, TotalValueM: 30m, GuaranteedM: 15m, SigningBonusM: 6m));

        using var db = factory.CreateDbContext();
        var old = await db.Contracts.FindAsync(oldContract.Id);
        Assert.False(old!.IsActive);
        Assert.True(old.IsModifiedBySim);
    }

    [Fact]
    public async Task ExtendPlayer_CreatesNewActiveContractWithCorrectYears()
    {
        var factory = MakeFactory();
        var player  = await SeedPlayerAsync(factory);
        await SeedContractAsync(factory, player.Id, 2, 8_000_000m, 2_000_000m);
        var svc = new RosterService(factory);

        await svc.ExtendPlayerAsync(player.Id, TeamId, Year,
            new ContractInput(Years: 3, TotalValueM: 30m, GuaranteedM: 15m, SigningBonusM: 6m));

        using var db = factory.CreateDbContext();
        var newContract = await db.Contracts
            .Include(c => c.ContractYears)
            .FirstOrDefaultAsync(c => c.PlayerId == player.Id && c.IsActive);

        Assert.NotNull(newContract);
        Assert.Equal(3, newContract!.ContractYears.Count);
        Assert.True(newContract.ContractYears.All(cy => cy.TeamId == TeamId));
        Assert.Equal(Year,     newContract.ContractYears.Min(cy => cy.Year));
        Assert.Equal(Year + 2, newContract.ContractYears.Max(cy => cy.Year));
    }

    [Fact]
    public async Task ExtendPlayer_NoExistingContract_StillCreatesNewOne()
    {
        var factory = MakeFactory();
        var player  = await SeedPlayerAsync(factory);
        var svc = new RosterService(factory);

        await svc.ExtendPlayerAsync(player.Id, TeamId, Year,
            new ContractInput(Years: 2, TotalValueM: 16m, GuaranteedM: 8m, SigningBonusM: 4m));

        using var db = factory.CreateDbContext();
        var count = await db.Contracts.CountAsync(c => c.PlayerId == player.Id && c.IsActive);
        Assert.Equal(1, count);
    }
}
