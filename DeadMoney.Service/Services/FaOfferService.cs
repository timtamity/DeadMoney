using DeadMoney.Core.Entities;
using DeadMoney.Core.Enums;
using DeadMoney.Data;
using Microsoft.EntityFrameworkCore;

namespace DeadMoney.Service.Services;

public record FaHeatDto(int PlayerId, int OfferCount, decimal MinApyM, decimal MaxApyM, decimal AvgApyM)
{
    public string HeatClass => OfferCount switch
    {
        0    => "heat-cold",
        1    => "heat-warm",
        2 or 3 => "heat-hot",
        _    => "heat-blazing"
    };

    public string HeatLabel => OfferCount switch
    {
        0 => "No offers",
        1 => "1 offer",
        _ => $"{OfferCount} offers"
    };

    public string RangeLabel => OfferCount == 0 ? "" :
        MinApyM == MaxApyM
            ? $"${MinApyM:F1}M/yr"
            : $"${MinApyM:F1}–{MaxApyM:F1}M/yr";

    // Where myApy falls in the range (0–100), clamped.
    public int CompetitivenessPercent(decimal myApyM)
    {
        if (MaxApyM <= MinApyM) return 50;
        var pct = (myApyM - MinApyM) / (MaxApyM - MinApyM) * 100m;
        return (int)Math.Clamp(pct, 0m, 100m);
    }

    public string CompetitivenessLabel(decimal myApyM) =>
        myApyM >= AvgApyM * 1.05m ? "Above market" :
        myApyM <= AvgApyM * 0.95m ? "Below market" :
        "At market";
}

public class FaOfferService
{
    private readonly IDbContextFactory<DeadMoneyDbContext> _dbFactory;

    public FaOfferService(IDbContextFactory<DeadMoneyDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task<FaOffer> SubmitOrUpdateAsync(int teamId, int playerId, int years, decimal totalValueM, decimal guaranteedM)
    {
        using var db = await _dbFactory.CreateDbContextAsync();

        var player = await db.Players.FindAsync(playerId);
        if (player == null || player.TeamId != null)
            throw new InvalidOperationException("Player is not a free agent.");

        var apyM = years > 0 ? totalValueM / years : 0m;

        var existing = await db.FaOffers
            .FirstOrDefaultAsync(o => o.TeamId == teamId && o.PlayerId == playerId && o.Status == FaOfferStatus.Active);

        if (existing != null)
        {
            existing.Years         = years;
            existing.TotalValueM   = totalValueM;
            existing.AnnualValueM  = apyM;
            existing.GuaranteedM   = guaranteedM;
            existing.SubmittedAt   = DateTime.UtcNow;
            await db.SaveChangesAsync();
            return existing;
        }

        var offer = new FaOffer
        {
            TeamId        = teamId,
            PlayerId      = playerId,
            Years         = years,
            TotalValueM   = totalValueM,
            AnnualValueM  = apyM,
            GuaranteedM   = guaranteedM,
            SubmittedAt   = DateTime.UtcNow
        };
        db.FaOffers.Add(offer);
        await db.SaveChangesAsync();
        return offer;
    }

    public async Task WithdrawAsync(int offerId, int teamId)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        var offer = await db.FaOffers
            .FirstOrDefaultAsync(o => o.Id == offerId && o.TeamId == teamId);
        if (offer == null) return;
        offer.Status = FaOfferStatus.Withdrawn;
        await db.SaveChangesAsync();
    }

    public async Task<FaHeatDto> GetHeatAsync(int playerId)
    {
        var batch = await GetHeatBatchAsync([playerId]);
        return batch.TryGetValue(playerId, out var h) ? h : new FaHeatDto(playerId, 0, 0, 0, 0);
    }

    public async Task<Dictionary<int, FaHeatDto>> GetHeatBatchAsync(IEnumerable<int> playerIds)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        var ids = playerIds.ToList();

        var groups = await db.FaOffers
            .Where(o => ids.Contains(o.PlayerId) && o.Status == FaOfferStatus.Active)
            .GroupBy(o => o.PlayerId)
            .Select(g => new
            {
                PlayerId = g.Key,
                Count    = g.Count(),
                MinApy   = g.Min(o => o.AnnualValueM),
                MaxApy   = g.Max(o => o.AnnualValueM),
                AvgApy   = g.Average(o => o.AnnualValueM)
            })
            .ToListAsync();

        return groups.ToDictionary(
            g => g.PlayerId,
            g => new FaHeatDto(g.PlayerId, g.Count, g.MinApy, g.MaxApy, g.AvgApy));
    }

    public async Task<List<FaOffer>> GetOffersForPlayerAsync(int playerId)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        return await db.FaOffers
            .Where(o => o.PlayerId == playerId && o.Status == FaOfferStatus.Active)
            .Include(o => o.Team)
            .OrderByDescending(o => o.AnnualValueM)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<FaOffer?> GetMyOfferAsync(int teamId, int playerId)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        return await db.FaOffers
            .FirstOrDefaultAsync(o => o.TeamId == teamId && o.PlayerId == playerId && o.Status == FaOfferStatus.Active);
    }
}
