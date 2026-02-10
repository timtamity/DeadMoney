using DeadMoney.Core.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DeadMoney.Data;

public class DeadMoneyDbContext : IdentityDbContext<IdentityUser>
{
    public DeadMoneyDbContext(DbContextOptions<DeadMoneyDbContext> options)
        : base(options)
    {
    }

    public DbSet<Team> Teams => Set<Team>();
    public DbSet<Player> Players => Set<Player>();
    public DbSet<Contract> Contracts => Set<Contract>();
    public DbSet<ContractYear> ContractYears => Set<ContractYear>();
    public DbSet<LeagueSetting> LeagueSettings => Set<LeagueSetting>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        // 1. MUST BE FIRST: This initializes the standard Identity tables
        base.OnModelCreating(builder);

        // 2. Loop through all entities to apply naming and schema rules
        foreach (var entity in builder.Model.GetEntityTypes())
        {
            var tableName = entity.GetTableName();

            // Check if it's an Identity table (starts with AspNet)
            if (tableName != null && tableName.StartsWith("AspNet"))
            {
                entity.SetSchema("Auth");
                // Remove the "AspNet" prefix (6 characters)
                entity.SetTableName(tableName.Substring(6));
            }
            else
            {
                // Move your NFL/Application tables to the "League" schema
                entity.SetSchema("League");
            }
        }

        // 3. Global Decimal Precision (18,2) for all financial values
        foreach (var property in builder.Model.GetEntityTypes()
            .SelectMany(t => t.GetProperties())
            .Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?)))
        {
            property.SetPrecision(18);
            property.SetScale(2);
        }

        // 4. Seed initial league data
        builder.SeedLeagueData();
    }
}