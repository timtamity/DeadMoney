namespace DeadMoney.Core.Entities;

public partial class Team
{
    // Total of all active contract cap hits for the current year
    public decimal TotalCapAllocated(int year) =>
        Roster.Sum(p => p.CurrentContract?.GetCapHitForYear(year) ?? 0);

    // OTC Style: Offseason cap is often calculated by the "Top 51" highest hits
    public decimal Top51CapHit(int year) =>
        Roster.Select(p => p.CurrentContract?.GetCapHitForYear(year) ?? 0)
              .OrderByDescending(h => h)
              .Take(51)
              .Sum();
}