using DeadMoney.Core.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DeadMoney.Data;

public class DeadMoneyDbContext : IdentityDbContext<ApplicationUser>
{
    public DeadMoneyDbContext(DbContextOptions<DeadMoneyDbContext> options)
        : base(options) { }

    public DbSet<Team> Teams => Set<Team>();
    public DbSet<Player> Players => Set<Player>();
    public DbSet<Position> Positions => Set<Position>();
    public DbSet<Contract> Contracts => Set<Contract>();
    public DbSet<ContractYear> ContractYears => Set<ContractYear>();
    public DbSet<LeagueSetting> LeagueSettings => Set<LeagueSetting>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Schema & Naming
        foreach (var entity in builder.Model.GetEntityTypes())
        {
            var tableName = entity.GetTableName();
            var entityNamespace = entity.ClrType.Namespace ?? string.Empty;

            // Handle Identity tables (User, Role, etc.)
            // We check for the AspNet prefix OR if the class lives in an Identity namespace
            if (tableName != null && (tableName.StartsWith("AspNet") || entityNamespace.Contains("Identity")))
            {
                entity.SetSchema("Auth");
                if (tableName.StartsWith("AspNet"))
                {
                    entity.SetTableName(tableName.Substring(6));
                }
            }
            else
            {
                entity.SetSchema("League");
            }
        }

        // Precision for Financials
        foreach (var property in builder.Model.GetEntityTypes()
            .SelectMany(t => t.GetProperties())
            .Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?)))
        {
            property.SetPrecision(18);
            property.SetScale(2);
        }

        // Store Enum as String
        builder.Entity<Position>()
            .Property(p => p.Unit)
            .HasConversion<string>();

        builder.SeedLeagueData();
    }
}