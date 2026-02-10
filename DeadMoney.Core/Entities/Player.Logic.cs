namespace DeadMoney.Core.Entities;

public partial class Player
{
    public string FullName => $"{FirstName} {LastName}";

    // Helper to grab the active deal without writing a query every time
    public Contract? CurrentContract => Contracts.FirstOrDefault(c => c.IsActive);

    public bool IsFreeAgent => TeamId == null;
}