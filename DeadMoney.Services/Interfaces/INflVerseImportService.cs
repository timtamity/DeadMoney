namespace DeadMoney.Services.Interfaces;

public interface INflVerseImportService
{
    /// <summary>
    /// Phase 1: Establish every player that exists in the nflverse ecosystem.
    /// Maps GSIS, OTC, and PFR IDs.
    /// </summary>
    Task SyncPlayerMasterListAsync();

    /// <summary>
    /// Phase 2: Updates current team assignments, numbers, and active status.
    /// </summary>
    Task SyncCurrentRostersAsync();

    /// <summary>
    /// Phase 3: Injects the OverTheCap financial baseline for all known players.
    /// </summary>
    Task SyncContractsAsync();
}