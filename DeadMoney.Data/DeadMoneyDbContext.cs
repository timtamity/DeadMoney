using DeadMoney.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace DeadMoney.Data;

public class DeadMoneyDbContext : DbContext
{
    public DeadMoneyDbContext(DbContextOptions<DeadMoneyDbContext> options)
        : base(options) { }

    // Auth Tables
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
        base.OnModelCreating(builder);

        // 1. Schema Strategy
        foreach (var entity in builder.Model.GetEntityTypes())
        {
            var authTypes = new[] { nameof(User), nameof(Role), nameof(UserRole) };
            entity.SetSchema(authTypes.Contains(entity.ClrType.Name) ? "Auth" : "League");
        }

        // 2. Financial Precision
        foreach (var property in builder.Model.GetEntityTypes()
            .SelectMany(t => t.GetProperties())
            .Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?)))
        {
            property.SetPrecision(18);
            property.SetScale(2);
        }

        // 3. Custom Constraints & Conversions
        builder.Entity<User>().HasIndex(u => u.DiscordId).IsUnique();
        builder.Entity<Position>().Property(p => p.Unit).HasConversion<string>();

        // 4. Many-to-Many: UserRole <-> Position
        // This explicitly defines the join table name and schema
        builder.Entity<UserRole>()
            .HasMany(ur => ur.Positions)
            .WithMany(p => p.UserRoles)
            .UsingEntity(j => j.ToTable("UserRolePositions", "Auth"));

        // 5. Seeding
        builder.SeedLeagueData();
    }
}