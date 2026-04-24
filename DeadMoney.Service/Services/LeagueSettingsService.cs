using DeadMoney.Core.Entities;
using DeadMoney.Data;
using Microsoft.EntityFrameworkCore;

namespace DeadMoney.Service.Services;

public class LeagueSettingsService
{
    private readonly IDbContextFactory<DeadMoneyDbContext> _dbFactory;

    public LeagueSettingsService(IDbContextFactory<DeadMoneyDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task<LeagueSetting?> GetCurrentAsync()
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        return await db.LeagueSettings.FirstOrDefaultAsync(s => s.IsCurrent);
    }

    public async Task<int> GetCurrentYearAsync()
    {
        var setting = await GetCurrentAsync();
        return setting?.Year ?? DateTime.UtcNow.Year;
    }

    public async Task<decimal> GetCurrentCapAsync()
    {
        var setting = await GetCurrentAsync();
        return setting?.SalaryCap ?? 279_200_000m;
    }

    public async Task<List<LeagueSetting>> GetAllAsync()
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        return await db.LeagueSettings.OrderByDescending(s => s.Year).AsNoTracking().ToListAsync();
    }

    public async Task SetCurrentYearAsync(int settingId)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        var all = await db.LeagueSettings.ToListAsync();
        foreach (var s in all)
            s.IsCurrent = s.Id == settingId;
        await db.SaveChangesAsync();
    }

    public async Task UpsertAsync(int year, decimal salaryCap)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        var existing = await db.LeagueSettings.FirstOrDefaultAsync(s => s.Year == year);
        if (existing != null)
        {
            existing.SalaryCap = salaryCap;
        }
        else
        {
            db.LeagueSettings.Add(new LeagueSetting { Year = year, SalaryCap = salaryCap, IsCurrent = false });
        }
        await db.SaveChangesAsync();
    }
}
