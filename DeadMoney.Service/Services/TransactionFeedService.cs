using DeadMoney.Core.Entities;
using DeadMoney.Core.Enums;
using DeadMoney.Data;
using Microsoft.EntityFrameworkCore;

namespace DeadMoney.Service.Services;

public record TransactionDto(
    int Id,
    TransactionType Type,
    string SubjectName,
    int? PlayerId,
    int? DraftPickId,
    string? TeamAbbr,
    int? TeamId,
    string? ToTeamAbbr,
    string? Details,
    DateTime OccurredAt);

public class TransactionFeedService
{
    private readonly IDbContextFactory<DeadMoneyDbContext> _dbFactory;
    private List<TransactionDto>? _feed;
    private readonly SemaphoreSlim _initLock = new(1, 1);

    public event Action? OnChange;

    public TransactionFeedService(IDbContextFactory<DeadMoneyDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task<IReadOnlyList<TransactionDto>> GetFeedAsync()
    {
        await EnsureLoadedAsync();
        return _feed!;
    }

    private async Task EnsureLoadedAsync()
    {
        if (_feed != null) return;
        await _initLock.WaitAsync();
        try
        {
            if (_feed != null) return;
            using var db = await _dbFactory.CreateDbContextAsync();
            var rows = await db.Transactions
                .Include(t => t.Player)
                .Include(t => t.Team)
                .Include(t => t.ToTeam)
                .Include(t => t.DraftPick)
                .OrderByDescending(t => t.OccurredAt)
                .Take(50)
                .AsNoTracking()
                .ToListAsync();
            _feed = rows.Select(ToDto).ToList();
        }
        finally { _initLock.Release(); }
    }

    public void Notify(TransactionDto dto)
    {
        _feed ??= new();
        lock (_feed)
        {
            _feed.Insert(0, dto);
            if (_feed.Count > 50) _feed.RemoveAt(50);
        }
        OnChange?.Invoke();
    }

    private static TransactionDto ToDto(Transaction t) => new(
        t.Id,
        t.Type,
        t.Player?.FullName ?? t.DraftPick?.Label ?? "Pick",
        t.PlayerId,
        t.DraftPickId,
        t.Team?.Abbreviation,
        t.TeamId,
        t.ToTeam?.Abbreviation,
        t.Details,
        t.OccurredAt);
}
