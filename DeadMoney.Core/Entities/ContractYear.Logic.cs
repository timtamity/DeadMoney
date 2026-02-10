namespace DeadMoney.Core.Entities;

public partial class ContractYear
{
    // The number that actually counts against the team's cap
    public decimal CapHit => BaseSalary + Bonuses + ProratedSigningBonus;

    // Logic for the UI to highlight if a salary is fully guaranteed
    public bool IsFullyGuaranteed => GuaranteedAmount >= BaseSalary;
}