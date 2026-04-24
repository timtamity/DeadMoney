using DeadMoney.Core.Enums;

namespace DeadMoney.Core.Entities;

public class PendingTrade
{
    public int Id { get; set; }

    public int TeamAId { get; set; }
    public virtual Team? TeamA { get; set; }

    public int TeamBId { get; set; }
    public virtual Team? TeamB { get; set; }

    public TradeStatus Status { get; set; } = TradeStatus.Proposed;
    public DateTime ProposedAt { get; set; } = DateTime.UtcNow;
    public string? Notes { get; set; }

    // Sending GM (Team A)
    public int? ProposedByUserId { get; set; }
    public string? ProposedByUserName { get; set; }

    // Receiving GM (Team B) — accept or decline
    public int? RespondedByUserId { get; set; }
    public string? RespondedByUserName { get; set; }
    public DateTime? RespondedAt { get; set; }

    // Commissioner — approve or reject
    public int? ReviewedByUserId { get; set; }
    public string? ReviewedByUserName { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? ReviewNotes { get; set; }

    public virtual ICollection<PendingTradeAsset> Assets { get; set; } = new List<PendingTradeAsset>();
}
