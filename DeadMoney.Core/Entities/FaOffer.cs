using DeadMoney.Core.Enums;

namespace DeadMoney.Core.Entities;

public class FaOffer
{
    public int Id { get; set; }
    public int PlayerId { get; set; }
    public Player? Player { get; set; }
    public int TeamId { get; set; }
    public Team? Team { get; set; }
    public int Years { get; set; }
    public decimal TotalValueM { get; set; }
    public decimal AnnualValueM { get; set; }
    public decimal GuaranteedM { get; set; }
    public FaOfferStatus Status { get; set; } = FaOfferStatus.Active;
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
    public string? Notes { get; set; }
}
