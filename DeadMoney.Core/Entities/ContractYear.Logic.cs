using System.ComponentModel.DataAnnotations.Schema;

namespace DeadMoney.Core.Entities;

public partial class ContractYear
{
    // Fixes the "missing Bonuses" error by summing the specific bonus buckets
    [NotMapped]
    public decimal TotalBonuses =>
        RosterBonus +
        OptionBonusProration +
        WorkoutBonus +
        MiscBonuses;

    // The number that actually counts against the team's cap
    [NotMapped]
    public decimal CapHit =>
        BaseSalary +
        TotalBonuses +
        SigningBonusProration;

    // Logic for the UI to highlight if a salary is fully guaranteed
    [NotMapped]
    public bool IsFullyGuaranteed => GuaranteedAmount >= (BaseSalary + RosterBonus);

    // Total cash the player actually pockets this year
    [NotMapped]
    public decimal TotalCash => BaseSalary + RosterBonus + WorkoutBonus + MiscBonuses;
}