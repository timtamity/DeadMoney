using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DeadMoney.Core.Entities;

[Table("UserRoles", Schema = "Auth")]
public class UserRole
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public virtual User User { get; set; } = null!;

    [Required]
    public int RoleId { get; set; }

    [ForeignKey(nameof(RoleId))]
    public virtual Role Role { get; set; } = null!;

    // The Payload: Nullable because Commissioners/Agents are League-wide.
    public int? TeamId { get; set; }

    [ForeignKey(nameof(TeamId))]
    public virtual Team? Team { get; set; }

    /// <summary>
    /// For Agents: The specific position they manage.
    /// If null, the role applies generally (e.g., a GM or a global Admin).
    /// </summary>
    public int? PositionId { get; set; }

    [ForeignKey(nameof(PositionId))]
    public virtual Position? Position { get; set; }

    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
}