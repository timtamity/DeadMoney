using DeadMoney.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.IO;

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

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Challenge: Ensure we don't double-configure if already set in Program.cs
        if (!optionsBuilder.IsConfigured)
        {
            // Hidden Secrets Strategy: 
            // In Production, this comes from Env Vars. 
            // In Local, this matches the secrets file we discussed.
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile("appsettings.secrets.json", optional: true) // The hidden file
                .AddEnvironmentVariables()
                .Build();

            var connectionString = config.GetConnectionString("DefaultConnection");
            optionsBuilder.UseSqlServer(connectionString);
        }
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // 1. Schema Strategy
        foreach (var entity in builder.Model.GetEntityTypes())
        {
            var authTypes = new[] { nameof(User), nameof(Role), nameof(UserRole) };
            entity.SetSchema(authTypes.Contains(entity.ClrType.Name) ? "Auth" : "League");
        }

        // 2. Financial Precision (Global Override)
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

        // 4. Contract Configuration
        builder.Entity<Contract>(entity =>
        {
            // Prevent deleting a Player from deleting all their historical contracts automatically 
            // unless explicitly intended. 
            entity.HasOne(c => c.Player)
                .WithMany(p => p.Contracts)
                .HasForeignKey(c => c.PlayerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Ensure the calculated Logic properties aren't accidentally mapped if [NotMapped] was missed
            entity.Ignore(c => c.APY);
            entity.Ignore(c => c.AnnualProration);
        });

        // 5. ContractYear Configuration
        builder.Entity<ContractYear>(entity =>
        {
            // Cascade delete IS appropriate here: If a contract is deleted, the years must go.
            entity.HasOne(cy => cy.Contract)
                .WithMany(c => c.ContractYears)
                .HasForeignKey(cy => cy.ContractId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Ignore(cy => cy.TotalBonuses);
            entity.Ignore(cy => cy.CapHit);
            entity.Ignore(cy => cy.IsFullyGuaranteed);
            entity.Ignore(cy => cy.TotalCash);
        });

        // 6. Many-to-Many: UserRole <-> Position
        builder.Entity<UserRole>()
            .HasMany(ur => ur.Positions)
            .WithMany(p => p.UserRoles)
            .UsingEntity(j => j.ToTable("UserRolePositions", "Auth"));

        // 7. Seeding
        builder.SeedLeagueData();
    }
}