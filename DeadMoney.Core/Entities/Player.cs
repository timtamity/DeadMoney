using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DeadMoney.Core.Entities;

public partial class Player
{
    [Key]
    public int Id { get; set; }

    // THE NEW ID CORE
    [MaxLength(50)]
    public string? GsisId { get; set; }  // NFL Official (e.g., 00-0036326)

    [MaxLength(50)]
    public string? OtcId { get; set; }   // OverTheCap Link

    [MaxLength(50)]
    public string? PfrId { get; set; }   // Pro-Football-Reference Link

    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    public string? Suffix { get; set; } // Jr., III, etc.

    [Required]
    public string PositionCode { get; set; } = string.Empty;

    public int? TeamId { get; set; }

    public bool IsRetired { get; set; }
    public string? College { get; set; }
    public string? BirthDate { get; set; }
    public int? Age { get; set; }
    public string? Height { get; set; }
    public int? Weight { get; set; }

    // Draft Metadata for SIM pedigree
    public int? DraftYear { get; set; }
    public int? DraftRound { get; set; }
    public int? DraftPick { get; set; }

    public string? YearsExp { get; set; }
    public string? Number { get; set; }
    public string? HeadshotUrl { get; set; }

    [ForeignKey(nameof(PositionCode))]
    public virtual Position Position { get; set; } = null!;

    [ForeignKey(nameof(TeamId))]
    public virtual Team? Team { get; set; }

    public virtual ICollection<Contract> Contracts { get; set; } = new List<Contract>();
}