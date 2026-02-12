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

    /// <summary>
    /// The team this player is associated with for this specific contract year.
    /// This is pulled from the team name in the nested data tuples.
    /// </summary>
    public int? TeamId { get; set; }

    [ForeignKey(nameof(TeamId))]
    public virtual Team? Team { get; set; }

    /// <summary>
    /// Indicates if this is a dummy year used for cap proration. 
    /// Base salaries for these years should be treated as 0.
    /// </summary>
    public bool IsVoidYear { get; set; }

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
    public decimal OtherBonus { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal PerGameRosterBonus { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal GuaranteedAmount { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal CapNumber { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal CashPaid { get; set; }
}