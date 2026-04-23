namespace DeadMoney.Web.Helpers;

public static class UiHelpers
{
    public static string PosBadgeClass(string? code) => (code?.ToUpper() ?? "") switch
    {
        "QB"                                    => "pos-badge pos-qb",
        "RB" or "FB" or "HB"                   => "pos-badge pos-rb",
        "WR"                                    => "pos-badge pos-wr",
        "TE"                                    => "pos-badge pos-te",
        "C"  or "G" or "T"                     => "pos-badge pos-ol",
        "DE" or "DT" or "NT"                   => "pos-badge pos-dl",
        "LB" or "ILB" or "OLB" or "MLB"
                     or "EDGE"                 => "pos-badge pos-lb",
        "CB" or "S"  or "DB"                   => "pos-badge pos-db",
        "K"  or "P"  or "LS"                   => "pos-badge pos-st",
        _                                       => "pos-badge"
    };
}
