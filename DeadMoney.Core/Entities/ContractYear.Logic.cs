using System.ComponentModel.DataAnnotations.Schema;

namespace DeadMoney.Core.Entities;

public partial class ContractYear
{
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