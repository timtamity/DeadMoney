namespace DeadMoney.Core.Entities;

public class LeagueSetting
{
    public int Id { get; set; }
    public int Year { get; set; }
    public decimal SalaryCap { get; set; }
    public bool IsCurrent { get; set; }
}