using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DeadMoney.Core.Entities;

public partial class ContractYear
{
    [Key]
    public int Id { get; set; }

    public int ContractId { get; set; }
    [ForeignKey(nameof(ContractId))]
    public virtual Contract Contract { get; set; } = null!;

    [Required]
    public int Year { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal BaseSalary { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal SigningBonusProration { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal RosterBonus { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal OptionBonusProration { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal WorkoutBonus { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal MiscBonuses { get; set; }

    /// <summary>
    /// Total amount of this specific year's compensation that is guaranteed.
    /// Used for Dead Money logic.
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal GuaranteedAmount { get; set; }
}