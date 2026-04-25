using DeadMoney.Core.Entities;
using DeadMoney.Core.Enums;
using DeadMoney.Data;
using Microsoft.EntityFrameworkCore;

namespace DeadMoney.Service.Services;

public record PickBoardRow(
    int    PickId,
    int    PickNumber,
    int    Round,
    int    TeamId,
    string TeamAbbr,
    string? TeamColor,
    int?   ProspectId,
    string? ProspectName,
    string? ProspectPosCode,
    bool   IsCurrent,
    bool   IsUsed);

public class DraftStateService : IDisposable
{
    private readonly IDbContextFactory<DeadMoneyDbContext> _dbFactory;
    private readonly SemaphoreSlim _opLock   = new(1, 1);
    private readonly SemaphoreSlim _initLock = new(1, 1);
    private Timer? _timer;
    private volatile bool _initialized;

    public event Action? OnChange;

    private DraftSessionStatus _status = DraftSessionStatus.NotStarted;
    private int? _sessionId;
    private int  _year;
    private int? _currentPickId;
    private int  _currentRound;
    private int  _currentPickNumber;
    private int? _onClockTeamId;
    private string  _onClockAbbr     = "";
    private string? _onClockColor;
    private string  _onClockCityName = "";
    private volatile int _secondsRemaining;
    private int  _secondsPerPick;
    private Dictionary<int, int> _roundClocks = new();
    private List<PickBoardRow>   _board        = new();

    public DraftSessionStatus        Status          => _status;
    public int?                      SessionId       => _sessionId;
    public int                       Year            => _year;
    public int?                      CurrentPickId   => _currentPickId;
    public int                       CurrentRound    => _currentRound;
    public int                       CurrentPickNumber => _currentPickNumber;
    public int?                      OnClockTeamId   => _onClockTeamId;
    public string                    OnClockAbbr     => _onClockAbbr;
    public string?                   OnClockColor    => _onClockColor;
    public string                    OnClockCityName => _onClockCityName;
    public int                       SecondsRemaining => _secondsRemaining;
    public int                       SecondsPerPick  => _secondsPerPick;
    public bool                      IsInitialized   => _initialized;
    public IReadOnlyList<PickBoardRow> Board          => _board;

    public DraftStateService(IDbContextFactory<DeadMoneyDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    // ─── Init ────────────────────────────────────────────────────────────────

    public async Task EnsureInitializedAsync()
    {
        if (_initialized) return;
        await _initLock.WaitAsync();
        try
        {
            if (_initialized) return;
            await LoadActiveSessionAsync();
            _initialized = true;
        }
        finally { _initLock.Release(); }
    }

    public async Task LoadSessionAsync(int sessionId)
    {
        await _opLock.WaitAsync();
        try
        {
            await LoadSessionCoreAsync(sessionId);
            _initialized = true;
        }
        finally { _opLock.Release(); }
        OnChange?.Invoke();
    }

    private async Task LoadActiveSessionAsync()
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        var session = await db.DraftSessions
            .Include(s => s.RoundClocks)
            .Include(s => s.CurrentPick).ThenInclude(p => p!.CurrentTeam)
            .Where(s => s.Status != DraftSessionStatus.Complete)
            .OrderByDescending(s => s.Id)
            .AsNoTracking()
            .FirstOrDefaultAsync();

        if (session == null) { _status = DraftSessionStatus.NotStarted; _board = new(); return; }

        ApplySession(session);
        await LoadBoardAsync(db, session.Year);
    }

    private async Task LoadSessionCoreAsync(int sessionId)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        var session = await db.DraftSessions
            .Include(s => s.RoundClocks)
            .Include(s => s.CurrentPick).ThenInclude(p => p!.CurrentTeam)
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == sessionId);

        if (session == null) return;
        ApplySession(session);
        await LoadBoardAsync(db, session.Year);
    }

    private void ApplySession(DraftSession session)
    {
        _status      = session.Status;
        _sessionId   = session.Id;
        _year        = session.Year;
        _roundClocks = session.RoundClocks.ToDictionary(rc => rc.Round, rc => rc.SecondsPerPick);

        var pick = session.CurrentPick;
        if (pick != null)
        {
            _currentPickId     = pick.Id;
            _currentRound      = pick.Round;
            _currentPickNumber = pick.PickNumber ?? 0;
            _onClockTeamId     = pick.CurrentTeamId;
            _onClockAbbr       = pick.CurrentTeam?.Abbreviation ?? "";
            _onClockColor      = pick.CurrentTeam?.PrimaryColor;
            _onClockCityName   = $"{pick.CurrentTeam?.City} {pick.CurrentTeam?.Nickname}".Trim();
            _secondsPerPick    = GetSecondsForRound(pick.Round);
            if (_secondsRemaining == 0) _secondsRemaining = _secondsPerPick;
        }
        else
        {
            _currentPickId = null;
            _onClockTeamId = null;
            _onClockAbbr   = "";
            _onClockColor  = null;
            _onClockCityName = "";
        }

        if (session.Status == DraftSessionStatus.Active) StartTimer();
        else StopTimer();
    }

    private async Task LoadBoardAsync(DeadMoneyDbContext db, int year)
    {
        var picks = await db.DraftPicks
            .Where(p => p.Year == year && !p.IsVoided)
            .Include(p => p.CurrentTeam)
            .Include(p => p.DraftProspect).ThenInclude(pr => pr!.Position)
            .OrderBy(p => p.PickNumber)
            .AsNoTracking()
            .ToListAsync();

        _board = picks.Select(p => new PickBoardRow(
            p.Id,
            p.PickNumber ?? 0,
            p.Round,
            p.CurrentTeamId,
            p.CurrentTeam?.Abbreviation ?? "?",
            p.CurrentTeam?.PrimaryColor,
            p.DraftProspectId,
            p.DraftProspect != null ? $"{p.DraftProspect.FirstName} {p.DraftProspect.LastName}" : null,
            p.DraftProspect?.Position?.Code,
            p.Id == _currentPickId,
            p.IsUsed
        )).ToList();
    }

    // ─── Controls ────────────────────────────────────────────────────────────

    public async Task StartAsync()
    {
        if (_sessionId == null) return;
        await _opLock.WaitAsync();
        try
        {
            using var db = await _dbFactory.CreateDbContextAsync();
            var session = await db.DraftSessions
                .Include(s => s.RoundClocks)
                .FirstOrDefaultAsync(s => s.Id == _sessionId);
            if (session == null) return;

            if (session.CurrentPickId == null)
            {
                var first = await db.DraftPicks
                    .Where(p => p.Year == session.Year && !p.IsUsed && !p.IsVoided)
                    .Include(p => p.CurrentTeam)
                    .OrderBy(p => p.PickNumber)
                    .FirstOrDefaultAsync();
                if (first == null) return;

                session.CurrentPickId  = first.Id;
                _currentPickId         = first.Id;
                _currentRound          = first.Round;
                _currentPickNumber     = first.PickNumber ?? 0;
                _onClockTeamId         = first.CurrentTeamId;
                _onClockAbbr           = first.CurrentTeam?.Abbreviation ?? "";
                _onClockColor          = first.CurrentTeam?.PrimaryColor;
                _onClockCityName       = $"{first.CurrentTeam?.City} {first.CurrentTeam?.Nickname}".Trim();
                _secondsPerPick        = GetSecondsForRound(first.Round);
                _secondsRemaining      = _secondsPerPick;
            }

            session.Status = DraftSessionStatus.Active;
            _status        = DraftSessionStatus.Active;
            await db.SaveChangesAsync();

            await LoadBoardAsync(db, _year);
            StartTimer();
        }
        finally { _opLock.Release(); }
        OnChange?.Invoke();
    }

    public async Task PauseAsync()
    {
        if (_sessionId == null) return;
        await _opLock.WaitAsync();
        try
        {
            StopTimer();
            _status = DraftSessionStatus.Paused;
            using var db = await _dbFactory.CreateDbContextAsync();
            var session  = await db.DraftSessions.FindAsync(_sessionId);
            if (session != null) { session.Status = DraftSessionStatus.Paused; await db.SaveChangesAsync(); }
        }
        finally { _opLock.Release(); }
        OnChange?.Invoke();
    }

    public async Task ResumeAsync()
    {
        if (_sessionId == null) return;
        await _opLock.WaitAsync();
        try
        {
            _status = DraftSessionStatus.Active;
            using var db = await _dbFactory.CreateDbContextAsync();
            var session  = await db.DraftSessions.FindAsync(_sessionId);
            if (session != null) { session.Status = DraftSessionStatus.Active; await db.SaveChangesAsync(); }
            StartTimer();
        }
        finally { _opLock.Release(); }
        OnChange?.Invoke();
    }

    public void ResetClock()
    {
        _secondsRemaining = _secondsPerPick;
        OnChange?.Invoke();
    }

    public async Task SubmitPickAsync(int prospectId, string pickedBy)
    {
        if (_currentPickId == null || _sessionId == null) return;
        await _opLock.WaitAsync();
        try
        {
            StopTimer();
            using var db = await _dbFactory.CreateDbContextAsync();

            var pick = await db.DraftPicks
                .Include(p => p.CurrentTeam)
                .FirstOrDefaultAsync(p => p.Id == _currentPickId);
            if (pick == null) return;

            var prospect = await db.DraftProspects.FindAsync(prospectId);
            if (prospect == null) return;

            pick.DraftProspectId = prospectId;
            pick.SelectedAt      = DateTime.UtcNow;
            pick.IsUsed          = true;
            prospect.IsDrafted   = true;

            db.Transactions.Add(new Transaction
            {
                Type                = TransactionType.DraftPickMade,
                TeamId              = pick.CurrentTeamId,
                DraftPickId         = pick.Id,
                OccurredAt          = DateTime.UtcNow,
                Details             = $"Round {pick.Round}, Pick #{pick.PickNumber}: {prospect.FirstName} {prospect.LastName}",
                PerformedByUserName = pickedBy
            });

            var nextPick = await db.DraftPicks
                .Where(p => p.Year == _year && !p.IsUsed && !p.IsVoided
                         && p.PickNumber > (pick.PickNumber ?? 0))
                .Include(p => p.CurrentTeam)
                .OrderBy(p => p.PickNumber)
                .FirstOrDefaultAsync();

            var session = await db.DraftSessions.FindAsync(_sessionId);
            if (session != null)
            {
                if (nextPick == null)
                {
                    session.Status        = DraftSessionStatus.Complete;
                    session.CurrentPickId = null;
                    _status        = DraftSessionStatus.Complete;
                    _currentPickId = null;
                    _onClockTeamId = null;
                    _onClockAbbr   = "";
                    _onClockColor  = null;
                    _onClockCityName = "";
                }
                else
                {
                    session.CurrentPickId  = nextPick.Id;
                    _currentPickId         = nextPick.Id;
                    _currentRound          = nextPick.Round;
                    _currentPickNumber     = nextPick.PickNumber ?? 0;
                    _onClockTeamId         = nextPick.CurrentTeamId;
                    _onClockAbbr           = nextPick.CurrentTeam?.Abbreviation ?? "";
                    _onClockColor          = nextPick.CurrentTeam?.PrimaryColor;
                    _onClockCityName       = $"{nextPick.CurrentTeam?.City} {nextPick.CurrentTeam?.Nickname}".Trim();
                    _secondsPerPick        = GetSecondsForRound(nextPick.Round);
                    _secondsRemaining      = _secondsPerPick;
                }
            }

            await db.SaveChangesAsync();
            await LoadBoardAsync(db, _year);

            if (_status == DraftSessionStatus.Active) StartTimer();
        }
        finally { _opLock.Release(); }
        OnChange?.Invoke();
    }

    public async Task RefreshBoardAsync()
    {
        await _opLock.WaitAsync();
        try
        {
            using var db = await _dbFactory.CreateDbContextAsync();
            await LoadBoardAsync(db, _year);
        }
        finally { _opLock.Release(); }
        OnChange?.Invoke();
    }

    // ─── Timer ───────────────────────────────────────────────────────────────

    private void StartTimer()
    {
        _timer?.Dispose();
        _timer = new Timer(OnTick, null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
    }

    private void StopTimer()
    {
        _timer?.Dispose();
        _timer = null;
    }

    private void OnTick(object? _)
    {
        if (_status != DraftSessionStatus.Active) return;
        if (_secondsRemaining > 0)
        {
            Interlocked.Decrement(ref _secondsRemaining);
            OnChange?.Invoke();
        }
    }

    private int GetSecondsForRound(int round) =>
        _roundClocks.TryGetValue(round, out var s) ? s : 300;

    public void Dispose()
    {
        _timer?.Dispose();
        _opLock.Dispose();
        _initLock.Dispose();
    }
}
