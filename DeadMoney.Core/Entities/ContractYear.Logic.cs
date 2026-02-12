using System.ComponentModel.DataAnnotations.Schema;

namespace DeadMoney.Core.Entities;

public partial class ContractYear
{
    // Fix: We use OtherBonus + PerGameRosterBonus to satisfy the logic 
    // that previously looked for "MiscBonuses"
    [NotMapped]
    public decimal TotalBonuses =>
        RosterBonus +
        WorkoutBonus +
        OtherBonus +
        PerGameRosterBonus;

    [NotMapped]
    public decimal CapHit =>
        BaseSalary +
        TotalBonuses +
        SigningBonusProration +
        OptionBonusProration;

    [NotMapped]
    public bool IsFullyGuaranteed => GuaranteedAmount >= (BaseSalary + RosterBonus);

    [NotMapped]
    public decimal TotalCash => BaseSalary + RosterBonus + WorkoutBonus + OtherBonus + PerGameRosterBonus;
}