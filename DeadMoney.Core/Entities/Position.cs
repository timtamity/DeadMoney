using System.ComponentModel.DataAnnotations;
using DeadMoney.Core.Enums;

namespace DeadMoney.Core.Entities;

public class Position
{
    [Key]
    [MaxLength(5)]
    public string Code { get; set; } = string.Empty; // e.g., "QB", "WR", "DE"

    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty; // e.g., "Quarterback"

    public int DisplayOrder { get; set; }

    // Using your preferred naming convention
    public PositionUnit Unit { get; set; }

    public virtual ICollection<Player> Players { get; set; } = new List<Player>();
}