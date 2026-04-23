using DeadMoney.Core.Entities;
using DeadMoney.Data;
using Microsoft.EntityFrameworkCore;

namespace DeadMoney.Service.Services;

public record ContractInput(int Years, decimal TotalValueM, decimal GuaranteedM, decimal SigningBonusM);

public class RosterService
{
    private readonly IDbContextFactory<DeadMoneyDbContext> _dbFactory;

    public RosterService(IDbContextFactory<DeadMoneyDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task CutPlayerAsync(int playerId, int teamId, int year, bool isPostJune1)
    {
        using var db = await _dbFactory.CreateDbContextAsync();

        var player = await db.Players
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

        player.TeamId = null;
        await db.SaveChangesAsync();
    }

    public async Task SignFreeAgentAsync(int playerId, int teamId, int year, ContractInput input)
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
        await db.SaveChangesAsync();
    }

    public async Task ExtendPlayerAsync(int playerId, int teamId, int year, ContractInput input)
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
        await db.SaveChangesAsync();
    }
}
