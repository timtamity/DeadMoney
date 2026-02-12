using System.Text.RegularExpressions;
using DeadMoney.Service.Models;

namespace DeadMoney.Service.Parsers;

public class NflVerseCsvParser
{
    // Updated Regex to capture Team Name at index 1
    private static readonly Regex YearTupleRegex = new Regex(
        @"\('(\d{4})','([^']+)',([\d\.\-]+),([\d\.\-]+),[^,]*,([\d\.\-]+),([\d\.\-]+)",
        RegexOptions.Compiled);

    public List<NflVerseYearDto> ParseNestedYears(string rawData)
    {
        var results = new List<NflVerseYearDto>();
        if (string.IsNullOrEmpty(rawData) || rawData == "\\N") return results;

        var matches = YearTupleRegex.Matches(rawData);
        foreach (Match m in matches)
        {
            results.Add(new NflVerseYearDto
            {
                Year = int.Parse(m.Groups[1].Value),
                TeamName = m.Groups[2].Value, // "Ravens", "Cowboys", etc.
                BaseSalary = ParseDecimal(m.Groups[3].Value),
                SigningBonusProration = ParseDecimal(m.Groups[4].Value),
                CapHit = ParseDecimal(m.Groups[6].Value)
            });
        }
        return results;
    }

    private decimal ParseDecimal(string val) =>
        decimal.TryParse(val, out var d) ? d : 0m;
}