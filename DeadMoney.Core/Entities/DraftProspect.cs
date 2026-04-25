using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DeadMoney.Core.Entities;

[Table("DraftProspects", Schema = "League")]
public class DraftProspect
{
    [Key]
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string FirstName { get; set; } = "";

    [Required, MaxLength(100)]
    public string LastName { get; set; } = "";

    public int PositionId { get; set; }
    public virtual Position Position { get; set; } = null!;

    public int Year { get; set; }

    [MaxLength(100)]
    public string? College { get; set; }

    [MaxLength(20)]
    public string? Height { get; set; }

    public int? Weight { get; set; }
    public int? Age    { get; set; }

    public bool IsDrafted { get; set; }

    public string FullName => $"{FirstName} {LastName}";
}
