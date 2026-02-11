using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DeadMoney.Core.Entities;

[Index(nameof(PlayerId))]
public partial class Contract
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int PlayerId { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal SigningBonus { get; set; }

    public bool IsActive { get; set; }

    /// <summary>
    /// If true, the NflVerse import will skip this record to avoid 
    /// overwriting a user's custom SIM scenario.
    /// </summary>
    public bool IsModifiedBySim { get; set; }

    // Navigation properties
    [ForeignKey(nameof(PlayerId))]
    public virtual Player Player { get; set; } = null!;

    public virtual List<ContractYear> ContractYears { get; set; } = new();
}