using System.ComponentModel.DataAnnotations.Schema;

namespace DeadMoney.Core.Entities
{
    [Table("UserRoles", Schema = "Auth")]
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

        /// <summary>
        /// For Agents: Specific positions they manage.
        /// For other roles, this collection will simply be empty.
        /// </summary>
        public virtual ICollection<Position> Positions { get; set; } = new List<Position>();

        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    }
}