using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DeadMoney.Core.Entities;

[Table("Teams", Schema = "League")]
public partial class Team
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(10)]
    public string Abbreviation { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string City { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Nickname { get; set; } = string.Empty;

    // --- New Structural Columns ---

    [Required]
    [MaxLength(10)]
    public string Conference { get; set; } = string.Empty; // e.g., "AFC", "NFC"

    [Required]
    [MaxLength(10)]
    public string Division { get; set; } = string.Empty; // e.g., "North", "South", "East", "West"

    // --- Aesthetic Columns ---

    [MaxLength(7)]
    public string? PrimaryColor { get; set; }

    [MaxLength(7)]
    public string? SecondaryColor { get; set; }

    [MaxLength(500)]
    public string? LogoUrl { get; set; }

    // --- Financial Columns ---

    [Column(TypeName = "decimal(18, 2)")]
    public decimal CarryoverCap { get; set; }

    // --- Navigation properties ---

    public virtual ICollection<Player> Roster { get; set; } = new List<Player>();

    // Links to the new UserRole table we created
    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}