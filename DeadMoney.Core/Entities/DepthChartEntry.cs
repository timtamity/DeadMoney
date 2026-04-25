using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DeadMoney.Core.Entities;

[Table("DepthChartEntries", Schema = "League")]
public class DepthChartEntry
{
    [Key]
    public int Id { get; set; }

    public int TeamId { get; set; }
    public virtual Team Team { get; set; } = null!;

    public int PlayerId { get; set; }
    public virtual Player Player { get; set; } = null!;

    [Required, MaxLength(10)]
    public string PositionCode { get; set; } = "";

    public int DepthOrder { get; set; }
}
