using System.ComponentModel.DataAnnotations;
using DeadMoney.Core.Enums;

namespace DeadMoney.Core.Entities;

public class Position
{
    [Key]
    [MaxLength(10)]
    public string Code { get; set; } = string.Empty; // "QB", "WR", etc.

    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty; // "Quarterback"

    public int DisplayOrder { get; set; }

    public PositionUnit Unit { get; set; }

    public virtual ICollection<Player> Players { get; set; } = new List<Player>();
}