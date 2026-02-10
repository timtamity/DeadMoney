namespace DeadMoney.Core.Entities;

public partial class ContractYear
{
    public int Id { get; set; }
    public int ContractId { get; set; }
    public int Year { get; set; }

    public decimal BaseSalary { get; set; }
    public decimal Bonuses { get; set; } // Roster, workout, etc.
    public decimal ProratedSigningBonus { get; set; }
    public decimal GuaranteedAmount { get; set; }
}