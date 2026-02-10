using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DeadMoney.Core.Entities;

public partial class Player
{
    [Key]
    public int Id { get; set; }

    [MaxLength(50)]
    public string? SleeperId { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    public string PositionCode { get; set; } = string.Empty;

    public int? TeamId { get; set; }

    public bool IsRetired { get; set; }
    public string? College { get; set; }
    public int? Age { get; set; }
    public string? Height { get; set; }
    public int? Weight { get; set; }
    public string? YearsExp { get; set; } // "R" for Rookie or a number
    public string? Number { get; set; } // Jersey Number

    [ForeignKey(nameof(PositionCode))]
    public virtual Position Position { get; set; } = null!;

    [ForeignKey(nameof(TeamId))]
    public virtual Team? Team { get; set; }

    public virtual ICollection<Contract> Contracts { get; set; } = new List<Contract>();
}