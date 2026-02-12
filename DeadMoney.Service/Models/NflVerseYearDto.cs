namespace DeadMoney.Service.Models;

public class NflVerseYearDto
{
    public int Year { get; set; }
    public string TeamName { get; set; } = string.Empty; // Direct from tuple index 1
    public decimal BaseSalary { get; set; }
    public decimal SigningBonusProration { get; set; }
    public decimal CapHit { get; set; }
}