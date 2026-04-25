using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DeadMoney.Core.Enums;

namespace DeadMoney.Core.Entities;

[Table("DraftSessions", Schema = "League")]
public class DraftSession
{
    [Key]
    public int Id { get; set; }

    public int Year { get; set; }

    public DraftSessionStatus Status { get; set; } = DraftSessionStatus.NotStarted;

    public int? CurrentPickId { get; set; }
    public virtual DraftPick? CurrentPick { get; set; }

    public virtual ICollection<DraftRoundClock> RoundClocks { get; set; } = new List<DraftRoundClock>();
}
