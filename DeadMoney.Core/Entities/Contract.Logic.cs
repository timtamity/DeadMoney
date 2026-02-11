using System.Linq;
using System.ComponentModel.DataAnnotations.Schema;

namespace DeadMoney.Core.Entities;

public partial class Contract
{
    [NotMapped]
    public decimal TotalValue => ContractYears.Sum(y =>
        y.BaseSalary +
        y.RosterBonus +
        y.WorkoutBonus +
        y.MiscBonuses
    ) + SigningBonus;

    [NotMapped]
    public decimal TotalGuaranteed => ContractYears.Sum(y => y.GuaranteedAmount) + SigningBonus;

    [NotMapped]
    public decimal APY => ContractYears.Count > 0 ? TotalValue / ContractYears.Count : 0;

    /// <summary>
    /// NFL Rule: Signing bonuses are prorated over the life of the contract, 
    /// but for a maximum of 5 years.
    /// </summary>
    [NotMapped]
    public decimal AnnualProration
    {
        get
        {
            if (ContractYears.Count == 0 || SigningBonus == 0) return 0;
            int prorationYears = Math.Min(ContractYears.Count, 5);
            return SigningBonus / prorationYears;
        }
    }

    public decimal GetCapHitForYear(int year)
    {
        var yearData = ContractYears.FirstOrDefault(y => y.Year == year);
        return yearData?.CapHit ?? 0;
    }

    public decimal GetDeadMoneyForYear(int year)
    {
        if (ContractYears.Count == 0) return 0;

        // 1. Calculate remaining proration
        // We only count years where proration actually applies (first 5 years of contract)
        var contractYearsOrdered = ContractYears.OrderBy(y => y.Year).ToList();
        int yearIndex = contractYearsOrdered.FindIndex(y => y.Year == year);

        decimal remainingProration = 0;
        if (yearIndex != -1 && yearIndex < 5)
        {
            int yearsOfProrationLeft = Math.Min(contractYearsOrdered.Count, 5) - yearIndex;
            remainingProration = AnnualProration * yearsOfProrationLeft;
        }

        // 2. Add future guaranteed salary/bonuses
        decimal futureGuarantees = ContractYears
            .Where(y => y.Year >= year)
            .Sum(y => y.GuaranteedAmount);

        return remainingProration + futureGuarantees;
    }

    public decimal GetSavingsForYear(int year)
    {
        return GetCapHitForYear(year) - GetDeadMoneyForYear(year);
    }
}