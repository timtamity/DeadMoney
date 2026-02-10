using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DeadMoney.Core.Entities;

public partial class Player
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    public string PositionCode { get; set; } = string.Empty;

    public int? TeamId { get; set; }

    public bool IsRetired { get; set; }

    [ForeignKey(nameof(PositionCode))]
    public virtual Position Position { get; set; } = null!;

    [ForeignKey(nameof(TeamId))]
    public virtual Team? Team { get; set; }

    public virtual ICollection<Contract> Contracts { get; set; } = new List<Contract>();
}