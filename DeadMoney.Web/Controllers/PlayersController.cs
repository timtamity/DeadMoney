using DeadMoney.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DeadMoney.Web.Controllers;

public class PlayersController(DeadMoneyDbContext context) : Controller
{
    public async Task<IActionResult> Index(string? search, int page = 1)
    {
        const int pageSize = 40; // Number of players per page

        // Use Identity Resolution to save memory on the 'Team' objects
        var query = context.Players
            .Include(p => p.Team)
            .AsNoTrackingWithIdentityResolution();

        if (!string.IsNullOrWhiteSpace(search))
        {
            // Case-insensitive search is default in SQL Server, but let's be safe
            query = query.Where(p =>
                p.FirstName.Contains(search) ||
                p.LastName.Contains(search));
        }

        // 1. Calculate the total count for the UI to know when to stop
        var totalPlayers = await query.CountAsync();

        // 2. Fetch only the "slice" of data for the current page
        var players = await query
            .OrderBy(p => p.LastName)
            .ThenBy(p => p.FirstName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        ViewBag.Search = search;
        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = (int)Math.Ceiling(totalPlayers / (double)pageSize);

        return View(players);
    }

    public async Task<IActionResult> Details(string id)
    {
        // For a single record, standard AsNoTracking is fine
        var player = await context.Players
            .Include(p => p.Team)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.SleeperId == id);

        if (player == null) return NotFound();

        return View(player);
    }
}