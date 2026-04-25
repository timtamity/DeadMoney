using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DeadMoney.Core.Entities;

[Table("DraftRoundClocks", Schema = "League")]
public class DraftRoundClock
{
    [Key]
    public int Id { get; set; }

    public int DraftSessionId { get; set; }
    public virtual DraftSession DraftSession { get; set; } = null!;

    public int Round { get; set; }

    public int SecondsPerPick { get; set; } = 300;
}
