using DeadMoney.Core.Entities;
using DeadMoney.Core.Enums;
using DeadMoney.Data;
using Microsoft.EntityFrameworkCore;

namespace DeadMoney.Service.Services;

public record TradeProposalResult(bool Success, string? Error);

public class TradeProposalService
{
    private readonly IDbContextFactory<DeadMoneyDbContext> _dbFactory;
    private readonly RosterService _roster;

    public TradeProposalService(IDbContextFactory<DeadMoneyDbContext> dbFactory, RosterService roster)
    {
        _dbFactory = dbFactory;
        _roster    = roster;
    }

    public async Task<TradeProposalResult> ProposeAsync(
        int teamAId, List<int> playerAIds, List<int> pickAIds,
        int teamBId, List<int> playerBIds, List<int> pickBIds,
        string? notes,
        int? proposedByUserId = null, string? proposedByUserName = null)
    {
        using var db = await _dbFactory.CreateDbContextAsync();

        var trade = new PendingTrade
        {
            TeamAId             = teamAId,
            TeamBId             = teamBId,
            Status              = TradeStatus.Proposed,
            ProposedAt          = DateTime.UtcNow,
            Notes               = notes,
            ProposedByUserId    = proposedByUserId,
            ProposedByUserName  = proposedByUserName
        };

        foreach (var pid in playerAIds)
            trade.Assets.Add(new PendingTradeAsset { SendingTeamId = teamAId, PlayerId = pid });
        foreach (var did in pickAIds)
            trade.Assets.Add(new PendingTradeAsset { SendingTeamId = teamAId, DraftPickId = did });
        foreach (var pid in playerBIds)
            trade.Assets.Add(new PendingTradeAsset { SendingTeamId = teamBId, PlayerId = pid });
        foreach (var did in pickBIds)
            trade.Assets.Add(new PendingTradeAsset { SendingTeamId = teamBId, DraftPickId = did });

        db.PendingTrades.Add(trade);
        await db.SaveChangesAsync();
        return new TradeProposalResult(true, null);
    }

    // Receiving GM (Team B) accepts → Proposed → Pending
    public async Task<TradeProposalResult> AcceptAsync(int tradeId, int teamId,
        int? respondedByUserId = null, string? respondedByUserName = null)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        var trade = await db.PendingTrades
            .FirstOrDefaultAsync(t => t.Id == tradeId && t.TeamBId == teamId && t.Status == TradeStatus.Proposed);

        if (trade == null)
            return new TradeProposalResult(false, "Trade not found or not in Proposed state.");

        trade.Status               = TradeStatus.Pending;
        trade.RespondedByUserId    = respondedByUserId;
        trade.RespondedByUserName  = respondedByUserName;
        trade.RespondedAt          = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return new TradeProposalResult(true, null);
    }

    // Receiving GM declines → Proposed → Declined
    public async Task<TradeProposalResult> DeclineAsync(int tradeId, int teamId,
        int? respondedByUserId = null, string? respondedByUserName = null)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        var trade = await db.PendingTrades
            .FirstOrDefaultAsync(t => t.Id == tradeId && t.TeamBId == teamId && t.Status == TradeStatus.Proposed);

        if (trade == null)
            return new TradeProposalResult(false, "Trade not found or not in Proposed state.");

        trade.Status               = TradeStatus.Declined;
        trade.RespondedByUserId    = respondedByUserId;
        trade.RespondedByUserName  = respondedByUserName;
        trade.RespondedAt          = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return new TradeProposalResult(true, null);
    }

    // Sending GM withdraws → Proposed or Pending → Withdrawn
    public async Task<TradeProposalResult> WithdrawAsync(int tradeId, int teamId,
        int? withdrawnByUserId = null, string? withdrawnByUserName = null)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        var trade = await db.PendingTrades
            .FirstOrDefaultAsync(t => t.Id == tradeId && t.TeamAId == teamId
                && (t.Status == TradeStatus.Proposed || t.Status == TradeStatus.Pending));

        if (trade == null)
            return new TradeProposalResult(false, "Trade not found or cannot be withdrawn.");

        trade.Status               = TradeStatus.Withdrawn;
        trade.RespondedByUserId    = withdrawnByUserId;
        trade.RespondedByUserName  = withdrawnByUserName;
        trade.RespondedAt          = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return new TradeProposalResult(true, null);
    }

    // Commissioner approves → Pending → Approved + executes
    public async Task<TradeProposalResult> ApproveAsync(int tradeId, int year,
        int? reviewedByUserId = null, string? reviewedByUserName = null, string? reviewNotes = null)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        var trade = await db.PendingTrades
            .Include(t => t.Assets)
            .FirstOrDefaultAsync(t => t.Id == tradeId && t.Status == TradeStatus.Pending);

        if (trade == null)
            return new TradeProposalResult(false, "Trade not found or not in Pending state.");

        trade.Status              = TradeStatus.Approved;
        trade.ReviewedByUserId    = reviewedByUserId;
        trade.ReviewedByUserName  = reviewedByUserName;
        trade.ReviewedAt          = DateTime.UtcNow;
        trade.ReviewNotes         = reviewNotes;
        await db.SaveChangesAsync();

        var playerAIds = trade.Assets.Where(a => a.SendingTeamId == trade.TeamAId && a.PlayerId.HasValue).Select(a => a.PlayerId!.Value).ToList();
        var pickAIds   = trade.Assets.Where(a => a.SendingTeamId == trade.TeamAId && a.DraftPickId.HasValue).Select(a => a.DraftPickId!.Value).ToList();
        var playerBIds = trade.Assets.Where(a => a.SendingTeamId == trade.TeamBId && a.PlayerId.HasValue).Select(a => a.PlayerId!.Value).ToList();
        var pickBIds   = trade.Assets.Where(a => a.SendingTeamId == trade.TeamBId && a.DraftPickId.HasValue).Select(a => a.DraftPickId!.Value).ToList();

        await _roster.ExecuteTradeAsync(
            trade.TeamAId, playerAIds, pickAIds,
            trade.TeamBId, playerBIds, pickBIds,
            year, reviewedByUserId, reviewedByUserName);

        return new TradeProposalResult(true, null);
    }

    // Commissioner rejects → Pending → Rejected
    public async Task<TradeProposalResult> RejectAsync(int tradeId,
        int? reviewedByUserId = null, string? reviewedByUserName = null, string? reviewNotes = null)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        var trade = await db.PendingTrades
            .FirstOrDefaultAsync(t => t.Id == tradeId && t.Status == TradeStatus.Pending);

        if (trade == null)
            return new TradeProposalResult(false, "Trade not found or not in Pending state.");

        trade.Status              = TradeStatus.Rejected;
        trade.ReviewedByUserId    = reviewedByUserId;
        trade.ReviewedByUserName  = reviewedByUserName;
        trade.ReviewedAt          = DateTime.UtcNow;
        trade.ReviewNotes         = reviewNotes;
        await db.SaveChangesAsync();
        return new TradeProposalResult(true, null);
    }

    public async Task<List<PendingTrade>> GetActiveAsync()
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        return await db.PendingTrades
            .Include(t => t.TeamA)
            .Include(t => t.TeamB)
            .Include(t => t.Assets).ThenInclude(a => a.Player).ThenInclude(p => p!.Position)
            .Include(t => t.Assets).ThenInclude(a => a.DraftPick).ThenInclude(dp => dp!.OriginalTeam)
            .Where(t => t.Status == TradeStatus.Proposed || t.Status == TradeStatus.Pending)
            .OrderByDescending(t => t.ProposedAt)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<List<PendingTrade>> GetForTeamAsync(int teamId)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        return await db.PendingTrades
            .Include(t => t.TeamA)
            .Include(t => t.TeamB)
            .Include(t => t.Assets).ThenInclude(a => a.Player).ThenInclude(p => p!.Position)
            .Include(t => t.Assets).ThenInclude(a => a.DraftPick).ThenInclude(dp => dp!.OriginalTeam)
            .Where(t => (t.TeamAId == teamId || t.TeamBId == teamId)
                     && (t.Status == TradeStatus.Proposed || t.Status == TradeStatus.Pending))
            .OrderByDescending(t => t.ProposedAt)
            .AsNoTracking()
            .ToListAsync();
    }
}
