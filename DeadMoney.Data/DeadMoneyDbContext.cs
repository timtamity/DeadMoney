using DeadMoney.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace DeadMoney.Data;

// CHANGE: Inherit from standard DbContext since Identity packages were removed
public class DeadMoneyDbContext : DbContext
{
    public DeadMoneyDbContext(DbContextOptions<DeadMoneyDbContext> options)
        : base(options) { }

    // Auth Tables (Custom Ground-Up Implementation)
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();

    // League Tables
    public DbSet<Team> Teams => Set<Team>();
    public DbSet<Player> Players => Set<Player>();
    public DbSet<Position> Positions => Set<Position>();
    public DbSet<Contract> Contracts => Set<Contract>();
    public DbSet<ContractYear> ContractYears => Set<ContractYear>();
    public DbSet<LeagueSetting> LeagueSettings => Set<LeagueSetting>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        // base.OnModelCreating(builder) is still good practice even without Identity
        base.OnModelCreating(builder);

        // Schema & Naming Strategy
        foreach (var entity in builder.Model.GetEntityTypes())
        {
            var entityNamespace = entity.ClrType.Namespace ?? string.Empty;

            // Organize by Namespace: Core.Entities vs Core.Entities.Auth (if you move them)
            // For now, we'll explicitly check the type names for the Auth schema
            var authTypes = new[] { nameof(User), nameof(Role), nameof(UserRole) };

            if (authTypes.Contains(entity.ClrType.Name))
            {
                entity.SetSchema("Auth");
            }
            else
            {
                entity.SetSchema("League");
            }
        }

        // Precision for Financials (Crucial for Salary Cap)
        foreach (var property in builder.Model.GetEntityTypes()
            .SelectMany(t => t.GetProperties())
            .Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?)))
        {
            property.SetPrecision(18);
            property.SetScale(2);
        }

        // Custom Constraints
        builder.Entity<User>()
            .HasIndex(u => u.DiscordId)
            .IsUnique();

        // Store Enum as String
        builder.Entity<Position>()
            .Property(p => p.Unit)
            .HasConversion<string>();

        // Seeding
        builder.SeedLeagueData();
    }
}