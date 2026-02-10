namespace DeadMoney.Core.Entities;

public partial class Player
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;

    public int? TeamId { get; set; }
    public Team? Team { get; set; }

    public List<Contract> Contracts { get; set; } = new();
}