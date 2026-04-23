using DeadMoney.Data;
using Microsoft.EntityFrameworkCore;

namespace DeadMoney.Tests.Helpers;

/// <summary>
/// Creates isolated in-memory EF Core contexts per test. Each instance gets a unique DB name
/// so tests don't share state. Call EnsureCreated() to apply HasData() seed (teams, positions).
/// </summary>
public sealed class TestDbContextFactory : IDbContextFactory<DeadMoneyDbContext>
{
    private readonly DbContextOptions<DeadMoneyDbContext> _options;

    public TestDbContextFactory(string? dbName = null)
    {
        _options = new DbContextOptionsBuilder<DeadMoneyDbContext>()
            .UseInMemoryDatabase(dbName ?? Guid.NewGuid().ToString())
            .Options;

        using var db = new DeadMoneyDbContext(_options);
        db.Database.EnsureCreated();
    }

    public DeadMoneyDbContext CreateDbContext() => new DeadMoneyDbContext(_options);

    // Extension-method compatibility for CreateDbContextAsync
    public Task<DeadMoneyDbContext> CreateDbContextAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(CreateDbContext());
}
