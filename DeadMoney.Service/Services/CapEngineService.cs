using DeadMoney.Core.Entities;
//using DeadMoney.Core.Models; // We'll create a Result model

namespace DeadMoney.Service.Services;

public class CapEngineService
{
    public TransactionResult SimulateCut(Contract contract, int currentYear, bool isPostJune1 = false)
    {
        var years = contract.ContractYears.OrderBy(y => y.Year).ToList();
        var currentYearData = years.FirstOrDefault(y => y.Year == currentYear);

        if (currentYearData == null) return new TransactionResult();

        // All future proration accelerates
        decimal totalDeadMoney = years
            .Where(y => y.Year >= currentYear)
            .Sum(y => y.SigningBonusProration + y.OptionBonusProration);

        // Add remaining guarantees (Logic simplified for now)
        // In a full suite, we'd check y.IsGuaranteed

        if (isPostJune1)
        {
            // Only current year proration hits now; the rest hits next year
            decimal currentYearDead = currentYearData.SigningBonusProration + currentYearData.OptionBonusProration;
            return new TransactionResult
            {
                DeadMoneyCurrent = currentYearDead,
                DeadMoneyFuture = totalDeadMoney - currentYearDead,
                CapSavings = currentYearData.CapNumber - currentYearDead
            };
        }

        return new TransactionResult
        {
            DeadMoneyCurrent = totalDeadMoney,
            CapSavings = currentYearData.CapNumber - totalDeadMoney
        };
    }
}

public class TransactionResult
{
    public decimal DeadMoneyCurrent { get; set; }
    public decimal DeadMoneyFuture { get; set; }
    public decimal CapSavings { get; set; }
}