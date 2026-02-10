namespace DeadMoney.Core.Entities;

public partial class Contract
{
    // Total cash value of the deal
    public decimal TotalValue => Years.Sum(y => y.BaseSalary + y.Bonuses) + SigningBonus;

    // Total guaranteed cash
    public decimal TotalGuaranteed => Years.Sum(y => y.GuaranteedAmount) + SigningBonus;

    // Average Per Year
    public decimal APY => Years.Count > 0 ? TotalValue / Years.Count : 0;
}