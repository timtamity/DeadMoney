using DeadMoney.Core.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace DeadMoney.Data;

public static class ModelBuilderExtensions
{
    public static void SeedLeagueData(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<LeagueSetting>().HasData(
            new LeagueSetting { Id = 1, Year = 2026, BaseSalaryCap = 303500000m }
        );

        modelBuilder.Entity<Team>().HasData(
            new Team { Id = 1, City = "Kansas City", Nickname = "Chiefs", Abbreviation = "KC" },
            new Team { Id = 2, City = "San Francisco", Nickname = "49ers", Abbreviation = "SF" }
            // ... add the rest of the 32 teams here
        );
    }
}