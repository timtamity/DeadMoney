using DeadMoney.Core.Enums;

namespace DeadMoney.Core.Entities;

public class Transaction
{
    public int Id { get; set; }
    public TransactionType Type { get; set; }
    public int? PlayerId { get; set; }
    public Player? Player { get; set; }
    public int? DraftPickId { get; set; }
    public DraftPick? DraftPick { get; set; }
    public int? TeamId { get; set; }
    public Team? Team { get; set; }
    public int? ToTeamId { get; set; }
    public Team? ToTeam { get; set; }
    public string? Details { get; set; }
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    public int? PerformedByUserId { get; set; }
    public string? PerformedByUserName { get; set; }
}
