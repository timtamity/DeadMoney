using System.Linq;
using System.ComponentModel.DataAnnotations.Schema;

namespace DeadMoney.Core.Entities;

public partial class Contract
{
    // Updated to use the correct fields from ContractYear
    [NotMapped]
    public decimal CalculatedTotalValue => ContractYears.Sum(y =>
        y.BaseSalary +
        y.RosterBonus +
        y.WorkoutBonus +
        y.OtherBonus +
        y.PerGameRosterBonus
    ) + SigningBonus;

    // Note: I renamed the property below to "CalculatedTotalValue" because your 
    // Contract.cs already contains a physical Column for "TotalValue". 
    // You cannot have a [NotMapped] property with the same name as a [Column].

    [NotMapped]
    public decimal CalculatedTotalGuaranteed => ContractYears.Sum(y => y.GuaranteedAmount) + SigningBonus;

    [NotMapped]
    public decimal APY => ContractYears.Count > 0 ? CalculatedTotalValue / ContractYears.Count : 0;

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
        if (yearData == null) return 0;

        var contractYearsOrdered = ContractYears.OrderBy(y => y.Year).ToList();
        int yearIndex = contractYearsOrdered.FindIndex(y => y.Year == year);

        decimal prorationSlice = (yearIndex >= 0 && yearIndex < 5) ? AnnualProration : 0;

        // Uses the logic properties from ContractYear
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

        var contractYearsOrdered = ContractYears.OrderBy(y => y.Year).ToList();
        int yearIndex = contractYearsOrdered.FindIndex(y => y.Year == year);

        if (yearIndex == -1) return 0;

        int totalProrationYears = Math.Min(contractYearsOrdered.Count, 5);
        decimal remainingProration = 0;

        if (yearIndex < totalProrationYears)
        {
            int yearsRemainingInProratedWindow = totalProrationYears - yearIndex;
            remainingProration = AnnualProration * yearsRemainingInProratedWindow;
        }

        decimal futureGuarantees = ContractYears
            .Where(y => y.Year >= year)
            .Sum(y => y.GuaranteedAmount);

        return remainingProration + futureGuarantees;
    }

    public decimal GetSavingsForYear(int year) => GetCapHitForYear(year) - GetDeadMoneyForYear(year);
}