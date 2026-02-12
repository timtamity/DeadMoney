using DeadMoney.Core.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DeadMoney.Core.Entities;

[Index(nameof(PlayerId))]
public partial class Contract
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int PlayerId { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalValue { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalGuaranteed { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal SigningBonus { get; set; }

    public bool IsActive { get; set; }
    public bool IsModifiedBySim { get; set; }

    [ForeignKey(nameof(PlayerId))]
    public virtual Player Player { get; set; } = null!;

    public virtual List<ContractYear> ContractYears { get; set; } = new();
}