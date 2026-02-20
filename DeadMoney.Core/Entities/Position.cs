using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DeadMoney.Core.Enums;

namespace DeadMoney.Core.Entities;

[Table("Positions", Schema = "League")]
public class Position
{
    [Key]
    public int Id { get; set; } // The Right Way: Integer PK

    [Required]
    [MaxLength(10)]
    public string Code { get; set; } = string.Empty; // "QB", "WR"

    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty; // "Quarterback"

    public int DisplayOrder { get; set; }

    public PositionUnit Unit { get; set; }

    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public virtual ICollection<Player> Players { get; set; } = new List<Player>();
}