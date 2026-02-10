namespace DeadMoney.Core.Entities;

public partial class Contract
{
    public int Id { get; set; }
    public int PlayerId { get; set; }
    public decimal SigningBonus { get; set; }
    public bool IsActive { get; set; }

    // Navigation property
    public List<ContractYear> Years { get; set; } = new();
}