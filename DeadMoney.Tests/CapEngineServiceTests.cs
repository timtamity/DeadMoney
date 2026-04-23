using DeadMoney.Core.Entities;
using DeadMoney.Service.Services;
using Xunit;

namespace DeadMoney.Tests;

public class CapEngineServiceTests
{
    private readonly CapEngineService _engine = new();

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static Contract MakeContract(int startYear, int years, decimal capPerYear, decimal sbProrationPerYear, decimal optionBonusPerYear = 0m)
    {
        var contract = new Contract { Id = 1, PlayerId = 1, IsActive = true };
        for (int i = 0; i < years; i++)
        {
            contract.ContractYears.Add(new ContractYear
            {
                Year                  = startYear + i,
                BaseSalary            = capPerYear - sbProrationPerYear - optionBonusPerYear,
                SigningBonusProration = sbProrationPerYear,
                OptionBonusProration  = optionBonusPerYear,
                CapNumber             = capPerYear
            });
        }
        return contract;
    }

    // ── Standard cut ─────────────────────────────────────────────────────────

    [Fact]
    public void SimulateCut_Standard_AcceleratesAllRemainingProration()
    {
        // 3-year deal: $7M cap/yr, $2M SB proration/yr — cutting in year 1
        var contract = MakeContract(2026, 3, capPerYear: 7_000_000m, sbProrationPerYear: 2_000_000m);

        var result = _engine.SimulateCut(contract, 2026);

        // Dead money = all 3 remaining years × $2M proration
        Assert.Equal(6_000_000m, result.DeadMoneyCurrent);
        Assert.Equal(0m,         result.DeadMoneyFuture);
        // Cap savings = current year cap hit − dead money
        Assert.Equal(1_000_000m, result.CapSavings);
    }

    [Fact]
    public void SimulateCut_Standard_CuttingMidContract_OnlyRemainingYearsAccelerate()
    {
        // 4-year deal, cutting in year 3 — only years 3 & 4 proration accelerates
        var contract = MakeContract(2024, 4, capPerYear: 10_000_000m, sbProrationPerYear: 3_000_000m);

        var result = _engine.SimulateCut(contract, 2026);

        Assert.Equal(6_000_000m, result.DeadMoneyCurrent); // 2 remaining years × $3M
        Assert.Equal(4_000_000m, result.CapSavings);        // $10M cap − $6M dead
    }

    [Fact]
    public void SimulateCut_Standard_NoProration_DeadMoneyIsZero()
    {
        var contract = MakeContract(2026, 3, capPerYear: 5_000_000m, sbProrationPerYear: 0m);

        var result = _engine.SimulateCut(contract, 2026);

        Assert.Equal(0m, result.DeadMoneyCurrent);
        Assert.Equal(5_000_000m, result.CapSavings);
    }

    [Fact]
    public void SimulateCut_Standard_IncludesOptionBonusProration()
    {
        // $1M SB proration + $0.5M option bonus proration per year
        var contract = MakeContract(2026, 2, capPerYear: 8_000_000m, sbProrationPerYear: 1_000_000m, optionBonusPerYear: 500_000m);

        var result = _engine.SimulateCut(contract, 2026);

        // Dead = 2 years × ($1M + $0.5M)
        Assert.Equal(3_000_000m, result.DeadMoneyCurrent);
    }

    [Fact]
    public void SimulateCut_Standard_NoCurrentYearData_ReturnsZeroResult()
    {
        var contract = MakeContract(2025, 2, capPerYear: 5_000_000m, sbProrationPerYear: 1_000_000m);

        // Cutting in a year not in the contract
        var result = _engine.SimulateCut(contract, 2028);

        Assert.Equal(0m, result.DeadMoneyCurrent);
        Assert.Equal(0m, result.DeadMoneyFuture);
        Assert.Equal(0m, result.CapSavings);
    }

    // ── Post-June 1 cut ──────────────────────────────────────────────────────

    [Fact]
    public void SimulateCut_PostJune1_CurrentYearProrationCountsNow()
    {
        var contract = MakeContract(2026, 3, capPerYear: 7_000_000m, sbProrationPerYear: 2_000_000m);

        var result = _engine.SimulateCut(contract, 2026, isPostJune1: true);

        Assert.Equal(2_000_000m, result.DeadMoneyCurrent); // only 2026 proration
    }

    [Fact]
    public void SimulateCut_PostJune1_FutureProrationDeferredToNextYear()
    {
        var contract = MakeContract(2026, 3, capPerYear: 7_000_000m, sbProrationPerYear: 2_000_000m);

        var result = _engine.SimulateCut(contract, 2026, isPostJune1: true);

        // Future = years 2027 + 2028 prorations combined
        Assert.Equal(4_000_000m, result.DeadMoneyFuture);
    }

    [Fact]
    public void SimulateCut_PostJune1_CapSavingsBasedOnCurrentYearOnly()
    {
        var contract = MakeContract(2026, 3, capPerYear: 7_000_000m, sbProrationPerYear: 2_000_000m);

        var result = _engine.SimulateCut(contract, 2026, isPostJune1: true);

        // Savings = current year cap hit − current year dead money
        Assert.Equal(5_000_000m, result.CapSavings);
    }

    [Fact]
    public void SimulateCut_PostJune1_SingleYearContract_SameAsStandard()
    {
        // Only one year left — no future prorations to defer
        var contract = MakeContract(2026, 1, capPerYear: 7_000_000m, sbProrationPerYear: 2_000_000m);

        var standard   = _engine.SimulateCut(contract, 2026, isPostJune1: false);
        var postJune1  = _engine.SimulateCut(contract, 2026, isPostJune1: true);

        Assert.Equal(standard.DeadMoneyCurrent, postJune1.DeadMoneyCurrent);
        Assert.Equal(0m, postJune1.DeadMoneyFuture);
        Assert.Equal(standard.CapSavings, postJune1.CapSavings);
    }

    [Fact]
    public void SimulateCut_PostJune1_TotalDeadMoneyEqualsStandardDeadMoney()
    {
        var contract = MakeContract(2026, 4, capPerYear: 10_000_000m, sbProrationPerYear: 2_500_000m);

        var standard  = _engine.SimulateCut(contract, 2026, isPostJune1: false);
        var postJune1 = _engine.SimulateCut(contract, 2026, isPostJune1: true);

        // Total dead money is the same — only the timing differs
        Assert.Equal(standard.DeadMoneyCurrent, postJune1.DeadMoneyCurrent + postJune1.DeadMoneyFuture);
    }
}
