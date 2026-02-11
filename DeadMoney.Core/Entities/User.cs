using System.ComponentModel.DataAnnotations;

namespace DeadMoney.Core.Entities
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string DiscordId { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Username { get; set; } = string.Empty;

        // --- Aesthetic Customization ---

        [MaxLength(100)]
        public string? DisplayName { get; set; }

        [MaxLength(500)]
        public string? AvatarUrl { get; set; } // Renamed from CustomAvatarUrl to match Service logic

        [MaxLength(500)]
        public string? Bio { get; set; }

        // --- UI & Localization Preferences ---

        [MaxLength(20)]
        public string ThemePreference { get; set; } = "Dark";

        public bool UseTeamColorsAsAccent { get; set; } = true;

        [Required]
        [MaxLength(100)]
        public string TimeZoneId { get; set; } = "UTC";

        // --- Metadata ---

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime LastLogin { get; set; } = DateTime.UtcNow;

        // Navigation property for our Many-to-Many roles
        public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    }
}