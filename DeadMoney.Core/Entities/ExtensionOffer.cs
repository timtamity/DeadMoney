using DeadMoney.Core.Enums;

namespace DeadMoney.Core.Entities;

public class ExtensionOffer
{
    public int Id { get; set; }

    public int PlayerId { get; set; }
    public virtual Player? Player { get; set; }

    public int TeamId { get; set; }
    public virtual Team? Team { get; set; }

    // GM who submitted the offer
    public int? OfferedByUserId { get; set; }
    public string? OfferedByUserName { get; set; }

    // GM's proposed terms (updated if they respond to a counter)
    public int Years { get; set; }
    public decimal TotalValueM { get; set; }
    public decimal AnnualValueM { get; set; }
    public decimal GuaranteedM { get; set; }
    public decimal SigningBonusM { get; set; }

    public ExtensionOfferStatus Status { get; set; } = ExtensionOfferStatus.Pending;
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
    public string? Notes { get; set; }

    // Agent/Commissioner review
    public int? ReviewedByUserId { get; set; }
    public string? ReviewedByUserName { get; set; }
    public DateTime? ReviewedAt { get; set; }

    // Counter terms (populated when Status == Countered)
    public int? CounterYears { get; set; }
    public decimal? CounterTotalValueM { get; set; }
    public decimal? CounterGuaranteedM { get; set; }
    public decimal? CounterSigningBonusM { get; set; }
    public string? CounterNotes { get; set; }
    public DateTime? CounteredAt { get; set; }
}
