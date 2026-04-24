using System.Linq;
using System.ComponentModel.DataAnnotations.Schema;

namespace DeadMoney.Core.Entities;

public partial class Contract
{
    [NotMapped]
    public decimal CalculatedTotalValue => ContractYears.Sum(y =>
        y.BaseSalary + y.RosterBonus + y.WorkoutBonus + y.OtherBonus + y.PerGameRosterBonus
    ) + SigningBonus;

    [NotMapped]
    public decimal CalculatedTotalGuaranteed => ContractYears.Sum(y => y.GuaranteedAmount) + SigningBonus;

    [NotMapped]
    public decimal APY => ContractYears.Count > 0 ? CalculatedTotalValue / ContractYears.Count : 0;

    // NFL signing bonus proration spreads over up to 5 years (league rule).
    [NotMapped]
    public decimal AnnualProration
    {
        get
        {
            if (ContractYears.Count == 0 || SigningBonus == 0) return 0;
            return SigningBonus / Math.Min(ContractYears.Count, 5);
        }
    }

    // Recomputes cap hit from stored component fields. For imported contracts use
    // ContractYear.CapHit (stored value); this method is used for cap simulation.
    public decimal GetCapHitForYear(int year)
    {
        var ordered = ContractYears.OrderBy(y => y.Year).ToList();
        int idx = ordered.FindIndex(y => y.Year == year);
        if (idx == -1) return 0;

        // Proration only applies within the first 5 contract years.
        var prorationSlice = idx < 5 ? AnnualProration : 0m;
        var yearData = ordered[idx];

        return yearData.BaseSalary +
               yearData.RosterBonus +
               yearData.WorkoutBonus +
               yearData.OtherBonus +
               yearData.PerGameRosterBonus +
               prorationSlice;
    }

    public decimal GetDeadMoneyForYear(int year)
    {
        if (ContractYears.Count == 0) return 0;

        var ordered = ContractYears.OrderBy(y => y.Year).ToList();
        int idx = ordered.FindIndex(y => y.Year == year);
        if (idx == -1) return 0;

        int prorationWindow = Math.Min(ordered.Count, 5);
        decimal remainingProration = idx < prorationWindow
            ? AnnualProration * (prorationWindow - idx)
            : 0m;

        decimal futureGuarantees = ContractYears
            .Where(y => y.Year >= year)
            .Sum(y => y.GuaranteedAmount);

        return remainingProration + futureGuarantees;
    }

    public decimal GetSavingsForYear(int year) => GetCapHitForYear(year) - GetDeadMoneyForYear(year);
}