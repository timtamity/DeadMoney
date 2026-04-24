using DeadMoney.Core.Entities;
using DeadMoney.Core.Enums;
using DeadMoney.Data;
using Microsoft.EntityFrameworkCore;

namespace DeadMoney.Service.Services;

public record ExtensionResult(bool Success, string? Error);

public class ExtensionService
{
    private readonly IDbContextFactory<DeadMoneyDbContext> _dbFactory;
    private readonly RosterService _roster;

    public ExtensionService(IDbContextFactory<DeadMoneyDbContext> dbFactory, RosterService roster)
    {
        _dbFactory = dbFactory;
        _roster    = roster;
    }

    // GM submits extension offer for a player on their team
    public async Task<ExtensionResult> SubmitAsync(
        int playerId, int teamId, int years, decimal totalValueM, decimal annualValueM,
        decimal guaranteedM, decimal signingBonusM, string? notes,
        int? offeredByUserId = null, string? offeredByUserName = null)
    {
        using var db = await _dbFactory.CreateDbContextAsync();

        var player = await db.Players.FindAsync(playerId);
        if (player == null || player.TeamId != teamId)
            return new ExtensionResult(false, "Player is not on this team.");

        var existing = await db.ExtensionOffers
            .AnyAsync(o => o.PlayerId == playerId && o.Status == ExtensionOfferStatus.Pending);
        if (existing)
            return new ExtensionResult(false, "A pending extension offer already exists for this player.");

        var offer = new ExtensionOffer
        {
            PlayerId            = playerId,
            TeamId              = teamId,
            OfferedByUserId     = offeredByUserId,
            OfferedByUserName   = offeredByUserName,
            Years               = years,
            TotalValueM         = totalValueM,
            AnnualValueM        = annualValueM,
            GuaranteedM         = guaranteedM,
            SigningBonusM       = signingBonusM,
            Notes               = notes,
            Status              = ExtensionOfferStatus.Pending,
            SubmittedAt         = DateTime.UtcNow
        };
        db.ExtensionOffers.Add(offer);

        db.Transactions.Add(new Transaction
        {
            Type                = TransactionType.ExtensionOffered,
            PlayerId            = playerId,
            TeamId              = teamId,
            Details             = $"Extension offered: {years}yr / ${totalValueM:F1}M",
            OccurredAt          = DateTime.UtcNow,
            PerformedByUserId   = offeredByUserId,
            PerformedByUserName = offeredByUserName
        });

        await db.SaveChangesAsync();
        return new ExtensionResult(true, null);
    }

    // Agent or Commissioner agrees to the extension — signs the player immediately
    public async Task<ExtensionResult> AgreeAsync(int offerId, int year,
        int? reviewedByUserId = null, string? reviewedByUserName = null)
    {
        using var db = await _dbFactory.CreateDbContextAsync();

        var offer = await db.ExtensionOffers
            .Include(o => o.Player)
            .FirstOrDefaultAsync(o => o.Id == offerId &&
                (o.Status == ExtensionOfferStatus.Pending || o.Status == ExtensionOfferStatus.Countered));

        if (offer == null)
            return new ExtensionResult(false, "Offer not found or not in a negotiable state.");

        offer.Status              = ExtensionOfferStatus.Agreed;
        offer.ReviewedByUserId    = reviewedByUserId;
        offer.ReviewedByUserName  = reviewedByUserName;
        offer.ReviewedAt          = DateTime.UtcNow;

        // Use counter terms if present, otherwise original
        var years        = offer.CounterYears        ?? offer.Years;
        var totalValueM  = offer.CounterTotalValueM  ?? offer.TotalValueM;
        var guaranteedM  = offer.CounterGuaranteedM  ?? offer.GuaranteedM;
        var signingBonusM = offer.CounterSigningBonusM ?? offer.SigningBonusM;

        db.Transactions.Add(new Transaction
        {
            Type                = TransactionType.ExtensionAgreed,
            PlayerId            = offer.PlayerId,
            TeamId              = offer.TeamId,
            Details             = $"Extension agreed: {years}yr / ${totalValueM:F1}M",
            OccurredAt          = DateTime.UtcNow,
            PerformedByUserId   = reviewedByUserId,
            PerformedByUserName = reviewedByUserName
        });

        await db.SaveChangesAsync();

        var input = new ContractInput(years, totalValueM, guaranteedM, signingBonusM);
        await _roster.ExtendPlayerAsync(offer.PlayerId, offer.TeamId, year, input, reviewedByUserId, reviewedByUserName);

        return new ExtensionResult(true, null);
    }

    // Agent or Commissioner counters with different terms
    public async Task<ExtensionResult> CounterAsync(int offerId,
        int counterYears, decimal counterTotalValueM, decimal counterGuaranteedM,
        decimal counterSigningBonusM, string? counterNotes,
        int? reviewedByUserId = null, string? reviewedByUserName = null)
    {
        using var db = await _dbFactory.CreateDbContextAsync();

        var offer = await db.ExtensionOffers
            .FirstOrDefaultAsync(o => o.Id == offerId &&
                (o.Status == ExtensionOfferStatus.Pending || o.Status == ExtensionOfferStatus.Countered));

        if (offer == null)
            return new ExtensionResult(false, "Offer not found or not in a negotiable state.");

        offer.Status                = ExtensionOfferStatus.Countered;
        offer.ReviewedByUserId      = reviewedByUserId;
        offer.ReviewedByUserName    = reviewedByUserName;
        offer.ReviewedAt            = DateTime.UtcNow;
        offer.CounterYears          = counterYears;
        offer.CounterTotalValueM    = counterTotalValueM;
        offer.CounterGuaranteedM    = counterGuaranteedM;
        offer.CounterSigningBonusM  = counterSigningBonusM;
        offer.CounterNotes          = counterNotes;
        offer.CounteredAt           = DateTime.UtcNow;

        db.Transactions.Add(new Transaction
        {
            Type                = TransactionType.ExtensionCountered,
            PlayerId            = offer.PlayerId,
            TeamId              = offer.TeamId,
            Details             = $"Extension countered: {counterYears}yr / ${counterTotalValueM:F1}M",
            OccurredAt          = DateTime.UtcNow,
            PerformedByUserId   = reviewedByUserId,
            PerformedByUserName = reviewedByUserName
        });

        await db.SaveChangesAsync();
        return new ExtensionResult(true, null);
    }

    // Agent or Commissioner declines
    public async Task<ExtensionResult> DeclineAsync(int offerId,
        int? reviewedByUserId = null, string? reviewedByUserName = null)
    {
        using var db = await _dbFactory.CreateDbContextAsync();

        var offer = await db.ExtensionOffers
            .FirstOrDefaultAsync(o => o.Id == offerId &&
                (o.Status == ExtensionOfferStatus.Pending || o.Status == ExtensionOfferStatus.Countered));

        if (offer == null)
            return new ExtensionResult(false, "Offer not found or not in a negotiable state.");

        offer.Status              = ExtensionOfferStatus.Declined;
        offer.ReviewedByUserId    = reviewedByUserId;
        offer.ReviewedByUserName  = reviewedByUserName;
        offer.ReviewedAt          = DateTime.UtcNow;

        db.Transactions.Add(new Transaction
        {
            Type                = TransactionType.ExtensionDeclined,
            PlayerId            = offer.PlayerId,
            TeamId              = offer.TeamId,
            Details             = $"Extension declined",
            OccurredAt          = DateTime.UtcNow,
            PerformedByUserId   = reviewedByUserId,
            PerformedByUserName = reviewedByUserName
        });

        await db.SaveChangesAsync();
        return new ExtensionResult(true, null);
    }

    // GM withdraws their own offer
    public async Task<ExtensionResult> WithdrawAsync(int offerId, int teamId,
        int? performedByUserId = null, string? performedByUserName = null)
    {
        using var db = await _dbFactory.CreateDbContextAsync();

        var offer = await db.ExtensionOffers
            .FirstOrDefaultAsync(o => o.Id == offerId && o.TeamId == teamId &&
                (o.Status == ExtensionOfferStatus.Pending || o.Status == ExtensionOfferStatus.Countered));

        if (offer == null)
            return new ExtensionResult(false, "Offer not found or cannot be withdrawn.");

        offer.Status = ExtensionOfferStatus.Withdrawn;

        db.Transactions.Add(new Transaction
        {
            Type                = TransactionType.ExtensionDeclined,
            PlayerId            = offer.PlayerId,
            TeamId              = offer.TeamId,
            Details             = "Extension withdrawn by GM",
            OccurredAt          = DateTime.UtcNow,
            PerformedByUserId   = performedByUserId,
            PerformedByUserName = performedByUserName
        });

        await db.SaveChangesAsync();
        return new ExtensionResult(true, null);
    }

    public async Task<List<ExtensionOffer>> GetPendingForPositionsAsync(IEnumerable<string> positionCodes)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        var codes = positionCodes.ToList();
        return await db.ExtensionOffers
            .Include(o => o.Player).ThenInclude(p => p!.Position)
            .Include(o => o.Player).ThenInclude(p => p!.Team)
            .Include(o => o.Team)
            .Where(o => (o.Status == ExtensionOfferStatus.Pending || o.Status == ExtensionOfferStatus.Countered)
                        && o.Player != null && codes.Contains(o.Player.Position!.Code))
            .OrderBy(o => o.SubmittedAt)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<List<ExtensionOffer>> GetAllPendingAsync()
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        return await db.ExtensionOffers
            .Include(o => o.Player).ThenInclude(p => p!.Position)
            .Include(o => o.Player).ThenInclude(p => p!.Team)
            .Include(o => o.Team)
            .Where(o => o.Status == ExtensionOfferStatus.Pending || o.Status == ExtensionOfferStatus.Countered)
            .OrderBy(o => o.SubmittedAt)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<List<ExtensionOffer>> GetForTeamAsync(int teamId)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        return await db.ExtensionOffers
            .Include(o => o.Player).ThenInclude(p => p!.Position)
            .Where(o => o.TeamId == teamId)
            .OrderByDescending(o => o.SubmittedAt)
            .AsNoTracking()
            .ToListAsync();
    }
}
