using DeadMoney.Core.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace DeadMoney.Data;

public class DeadMoneyDbContext : IdentityDbContext
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
        base.OnModelCreating(builder);

        // 1. Global Decimal Precision
        // NFL contracts are massive. We use (18,2) which allows for 
        // up to $999,999,999,999,999.99 (Quadrillions).
        foreach (var property in builder.Model.GetEntityTypes()
            .SelectMany(t => t.GetProperties())
            .Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?)))
        {
            property.SetPrecision(18);
            property.SetScale(2);
        }

        // 2. Relationship Mapping
        builder.Entity<Player>()
            .HasOne(p => p.Team)
            .WithMany(t => t.Roster)
            .HasForeignKey(p => p.TeamId)
            .OnDelete(DeleteBehavior.SetNull); // If a team is deleted, players become Free Agents

        // 3. Data Seeding
        // We call an extension method to keep this file clean.
        builder.SeedLeagueData();
    }
}