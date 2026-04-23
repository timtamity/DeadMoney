namespace DeadMoney.Core.Entities;

public class DraftPick
{
    public int Id { get; set; }
    public int Year { get; set; }
    public int Round { get; set; }
    public int? PickNumber { get; set; }
    public int OriginalTeamId { get; set; }
    public Team? OriginalTeam { get; set; }
    public int CurrentTeamId { get; set; }
    public Team? CurrentTeam { get; set; }
    public bool IsUsed { get; set; }
    public bool IsVoided { get; set; }
    public string? Notes { get; set; }

    public string Label => PickNumber.HasValue
        ? $"{Year} Round {Round} (#{PickNumber})"
        : $"{Year} {RoundSuffix(Round)} Round";

    private static string RoundSuffix(int r) => r switch
    {
        1 => "1st",
        2 => "2nd",
        3 => "3rd",
        _ => $"{r}th"
    };
}
