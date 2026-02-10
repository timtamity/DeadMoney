namespace DeadMoney.Core.Entities;

public partial class Team
{
    public int Id { get; set; }
    public string City { get; set; } = string.Empty;
    public string Nickname { get; set; } = string.Empty;
    public string Abbreviation { get; set; } = string.Empty;

    // Every team has their own carryover from the previous year
    public decimal CarryoverCap { get; set; }

    // Navigation properties
    public List<Player> Roster { get; set; } = new();
}