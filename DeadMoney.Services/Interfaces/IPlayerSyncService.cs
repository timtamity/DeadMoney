namespace DeadMoney.Services.Interfaces;

public interface IPlayerSyncService
{
    /// <summary>
    /// Pulls player data from an external API (Sleeper) and upserts 
    /// the records into the local database.
    /// </summary>
    Task SyncPlayersAsync();
}