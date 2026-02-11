using System.ComponentModel.DataAnnotations;

namespace DeadMoney.Core.Entities
{
    public class Role
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = string.Empty; // e.g., "Commissioner", "GM"

        [MaxLength(250)]
        public string Description { get; set; } = string.Empty;

        // Navigation property
        public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    }
}