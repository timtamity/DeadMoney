using DeadMoney.Core.Entities;

namespace DeadMoney.Service.Services;

public record TransactionResult(
    decimal DeadMoneyCurrent = 0m,
    decimal DeadMoneyFuture  = 0m,
    decimal CapSavings       = 0m);

public class CapEngineService
{
    public TransactionResult SimulateCut(Contract contract, int currentYear, bool isPostJune1 = false)
    {
        var years = contract.ContractYears.OrderBy(y => y.Year).ToList();
        var currentYearData = years.FirstOrDefault(y => y.Year == currentYear);
        if (currentYearData == null) return new TransactionResult();

        decimal totalProration = years
            .Where(y => y.Year >= currentYear)
            .Sum(y => y.SigningBonusProration + y.OptionBonusProration);

        if (isPostJune1)
        {
            decimal currentYearDead = currentYearData.SigningBonusProration + currentYearData.OptionBonusProration;
            return new TransactionResult(
                DeadMoneyCurrent: currentYearDead,
                DeadMoneyFuture:  totalProration - currentYearDead,
                CapSavings:       currentYearData.CapNumber - currentYearDead);
        }

        return new TransactionResult(
            DeadMoneyCurrent: totalProration,
            CapSavings:       currentYearData.CapNumber - totalProration);
    }
}