using System.ComponentModel.DataAnnotations.Schema;

namespace DeadMoney.Core.Entities
{
    public class UserRole
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        public int RoleId { get; set; }
        [ForeignKey("RoleId")]
        public virtual Role Role { get; set; } = null!;

        // The Payload: Nullable because Commissioners/Agents are League-wide.
        // GMs and Assistant GMs will have a value here.
        public int? TeamId { get; set; }

        [ForeignKey("TeamId")]
        public virtual Team? Team { get; set; }

        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    }
}