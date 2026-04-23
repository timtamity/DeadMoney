using DeadMoney.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace DeadMoney.Data;

public class DeadMoneyDbContext : DbContext
{
    public DeadMoneyDbContext(DbContextOptions<DeadMoneyDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<Player> Players => Set<Player>();
    public DbSet<Position> Positions => Set<Position>();
    public DbSet<Team> Teams => Set<Team>();
    public DbSet<Contract> Contracts => Set<Contract>();
    public DbSet<ContractYear> ContractYears => Set<ContractYear>();
    public DbSet<LeagueSetting> LeagueSettings => Set<LeagueSetting>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // 1. UserRole Mapping
        builder.Entity<UserRole>(entity =>
        {
            entity.ToTable("UserRoles", "Auth");
            entity.HasKey(ur => ur.Id);

            entity.HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            // Using WithMany(t => t.UserRoles) prevents the creation of shadow 'TeamId1'
            entity.HasOne(ur => ur.Team)
                .WithMany(t => t.UserRoles)
                .HasForeignKey(ur => ur.TeamId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            // Using WithMany(p => p.UserRoles) prevents the creation of shadow 'PositionId1'
            entity.HasOne(ur => ur.Position)
                .WithMany(p => p.UserRoles)
                .HasForeignKey(ur => ur.PositionId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // 2. League Mappings
        builder.Entity<Position>(entity =>
        {
            entity.ToTable("Positions", "League");
        });

        builder.Entity<Team>(entity =>
        {
            entity.ToTable("Teams", "League");
        });

        builder.Entity<Player>(entity =>
        {
            entity.ToTable("Players", "League");
            entity.HasOne(p => p.Position)
                .WithMany(pos => pos.Players)
                .HasForeignKey(p => p.PositionId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(p => p.Team)
                .WithMany(t => t.Roster)
                .HasForeignKey(p => p.TeamId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // 3. Contract Mappings
        builder.Entity<Contract>(entity =>
        {
            entity.ToTable("Contracts", "League");

            // Matches the collection name in your Contract entity
            entity.HasMany(c => c.ContractYears)
                .WithOne(y => y.Contract)
                .HasForeignKey(y => y.ContractId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<ContractYear>(entity =>
        {
            entity.ToTable("ContractYears", "League");
        });

        builder.Entity<LeagueSetting>(entity =>
        {
            entity.ToTable("LeagueSettings", "League");
            entity.Property(s => s.SalaryCap).HasPrecision(18, 2);
        });

        // 4. Static Seeding
        builder.SeedLeagueData();
    }
}