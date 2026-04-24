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
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<DraftPick> DraftPicks => Set<DraftPick>();
    public DbSet<FaOffer> FaOffers => Set<FaOffer>();

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

            entity.HasIndex(p => p.IsRetired);
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
            entity.HasIndex(cy => new { cy.TeamId, cy.Year });
        });

        builder.Entity<LeagueSetting>(entity =>
        {
            entity.ToTable("LeagueSettings", "League");
            entity.Property(s => s.SalaryCap).HasPrecision(18, 2);
        });

        builder.Entity<Transaction>(entity =>
        {
            entity.ToTable("Transactions", "League");
            entity.HasIndex(t => t.OccurredAt);
            entity.HasOne(t => t.Player)
                  .WithMany()
                  .HasForeignKey(t => t.PlayerId)
                  .IsRequired(false)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(t => t.Team)
                  .WithMany()
                  .HasForeignKey(t => t.TeamId)
                  .IsRequired(false)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(t => t.ToTeam)
                  .WithMany()
                  .HasForeignKey(t => t.ToTeamId)
                  .IsRequired(false)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(t => t.DraftPick)
                  .WithMany()
                  .HasForeignKey(t => t.DraftPickId)
                  .IsRequired(false)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<DraftPick>(entity =>
        {
            entity.ToTable("DraftPicks", "League");
            entity.HasOne(dp => dp.OriginalTeam)
                  .WithMany()
                  .HasForeignKey(dp => dp.OriginalTeamId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(dp => dp.CurrentTeam)
                  .WithMany()
                  .HasForeignKey(dp => dp.CurrentTeamId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<FaOffer>(entity =>
        {
            entity.ToTable("FaOffers", "League");
            entity.HasIndex(o => new { o.PlayerId, o.Status });
            entity.HasIndex(o => new { o.TeamId,   o.PlayerId });
            entity.Property(o => o.TotalValueM).HasPrecision(18, 2);
            entity.Property(o => o.AnnualValueM).HasPrecision(18, 2);
            entity.Property(o => o.GuaranteedM).HasPrecision(18, 2);
            entity.HasOne(o => o.Player)
                  .WithMany()
                  .HasForeignKey(o => o.PlayerId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(o => o.Team)
                  .WithMany()
                  .HasForeignKey(o => o.TeamId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // 4. Static Seeding
        builder.SeedLeagueData();
    }
}