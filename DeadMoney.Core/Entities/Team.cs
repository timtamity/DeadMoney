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

    // New Columns
    [MaxLength(7)]
    public string? PrimaryColor { get; set; }

    [MaxLength(7)]
    public string? SecondaryColor { get; set; }

    [MaxLength(500)]
    public string? LogoUrl { get; set; }

    // Every team has their own carryover from the previous year
    public decimal CarryoverCap { get; set; }

    // Navigation properties
    public List<Player> Roster { get; set; } = new();
}