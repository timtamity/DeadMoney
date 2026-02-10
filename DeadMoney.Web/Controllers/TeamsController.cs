using DeadMoney.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DeadMoney.Web.Controllers;

public class TeamsController(DeadMoneyDbContext context) : Controller
{
    // GET: /Teams - List all 32 NFL Teams
    public async Task<IActionResult> Index()
    {
        var teams = await context.Teams
            .AsNoTracking()
            .OrderBy(t => t.Name)
            .ToListAsync();

        return View(teams);
    }

    // GET: /Teams/Roster/DAL - Show roster for a specific team
    public async Task<IActionResult> Roster(string id) // id is Abbreviation
    {
        var team = await context.Teams
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Abbreviation == id);

        if (team == null) return NotFound();

        // Get all players for this team, sorted by Last Name
        var players = await context.Players
            .Where(p => p.TeamId == team.Id)
            .OrderBy(p => p.LastName)
            .AsNoTracking()
            .ToListAsync();

        ViewBag.TeamName = team.Name;
        ViewBag.TeamColor = team.PrimaryColor;
        ViewBag.TeamLogo = team.LogoUrl;

        return View(players);
    }
}