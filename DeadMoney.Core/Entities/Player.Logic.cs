using System.ComponentModel.DataAnnotations.Schema;

namespace DeadMoney.Core.Entities;

public partial class Player
{
    [NotMapped]
    public string FullName => string.IsNullOrWhiteSpace(Suffix)
        ? $"{FirstName} {LastName}"
        : $"{FirstName} {LastName} {Suffix}";

    [NotMapped]
    public Contract? CurrentContract => Contracts.FirstOrDefault(c => c.IsActive);

    [NotMapped]
    public bool IsFreeAgent => TeamId == null;

    [NotMapped]
    public string DisplayExperience => YearsExp == "0" || YearsExp == "R" ? "Rookie" : $"{YearsExp} Years";
}