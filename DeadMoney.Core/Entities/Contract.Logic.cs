using System.Linq;

namespace DeadMoney.Core.Entities;

public partial class Contract
{
    // Total cash value of the deal
    public decimal TotalValue => Years.Sum(y => y.BaseSalary + y.Bonuses) + SigningBonus;

    // Total guaranteed cash
    public decimal TotalGuaranteed => Years.Sum(y => y.GuaranteedAmount) + SigningBonus;

    // Average Per Year
    public decimal APY => Years.Count > 0 ? TotalValue / Years.Count : 0;

    /// <summary>
    /// Calculates the cap hit for a specific league year.
    /// This is what your Team.Logic.cs is looking for!
    /// </summary>
    public decimal GetCapHitForYear(int year)
    {
        // We find the specific year entry in the Years collection.
        // If the contract doesn't cover that year, the cap hit is 0.
        var yearData = Years.FirstOrDefault(y => y.Year == year);

        return yearData?.CapHit ?? 0;
    }
}