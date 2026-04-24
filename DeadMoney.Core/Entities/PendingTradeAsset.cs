namespace DeadMoney.Core.Entities;

public class PendingTradeAsset
{
    public int Id { get; set; }

    public int PendingTradeId { get; set; }
    public virtual PendingTrade Trade { get; set; } = null!;

    // Which team is giving up this asset
    public int SendingTeamId { get; set; }

    public int? PlayerId { get; set; }
    public virtual Player? Player { get; set; }

    public int? DraftPickId { get; set; }
    public virtual DraftPick? DraftPick { get; set; }
}
