using DeadMoney.Core.Enums;

namespace DeadMoney.Web.Helpers;

public static class UiHelpers
{
    public static string PosBadgeClass(string? code) => (code?.ToUpper() ?? "") switch
    {
        "QB"                                    => "pos-badge pos-qb",
        "RB" or "FB" or "HB"                   => "pos-badge pos-rb",
        "WR"                                    => "pos-badge pos-wr",
        "TE"                                    => "pos-badge pos-te",
        "C"  or "G" or "OT"                    => "pos-badge pos-ol",
        "DT" or "NT"                            => "pos-badge pos-dl",
        "LB" or "ILB" or "OLB" or "MLB"
                     or "EDGE"                 => "pos-badge pos-lb",
        "CB" or "S"  or "DB"                   => "pos-badge pos-db",
        "K"  or "P"  or "LS"                   => "pos-badge pos-st",
        _                                       => "pos-badge"
    };

    public static string TxnLabel(TransactionType t) => t switch
    {
        TransactionType.Cut                => "Cut",
        TransactionType.Signed             => "Signed",
        TransactionType.Extended           => "Extended",
        TransactionType.Traded             => "Traded",
        TransactionType.PickGranted        => "Pick Granted",
        TransactionType.PickForfeited      => "Pick Forfeited",
        TransactionType.ContractModified   => "Contract Modified",
        TransactionType.CapAdjusted        => "Cap Adjusted",
        TransactionType.ExtensionOffered   => "Extension Offered",
        TransactionType.ExtensionAgreed    => "Extension Agreed",
        TransactionType.ExtensionCountered => "Counter Offer",
        TransactionType.ExtensionDeclined  => "Extension Declined",
        TransactionType.FaOfferRejected    => "Offer Rejected",
        TransactionType.DraftPickMade      => "Drafted",
        _                                  => t.ToString()
    };

    public static string TxnIcon(TransactionType t) => t switch
    {
        TransactionType.Cut                => "✂",
        TransactionType.Signed             => "+",
        TransactionType.Extended           => "↑",
        TransactionType.Traded             => "⇄",
        TransactionType.PickGranted        => "⊕",
        TransactionType.PickForfeited      => "✕",
        TransactionType.DraftPickMade      => "⬆",
        _                                  => "·"
    };

    public static string TxnClass(TransactionType t) => t switch
    {
        TransactionType.Cut                => "txn-cut",
        TransactionType.Signed             => "txn-signed",
        TransactionType.Extended           => "txn-extended",
        TransactionType.Traded             => "txn-traded",
        TransactionType.PickGranted        => "txn-signed",
        TransactionType.PickForfeited      => "txn-cut",
        TransactionType.ContractModified   => "txn-extended",
        TransactionType.CapAdjusted        => "txn-traded",
        TransactionType.ExtensionOffered   => "txn-extended",
        TransactionType.ExtensionAgreed    => "txn-signed",
        TransactionType.ExtensionCountered => "txn-traded",
        TransactionType.ExtensionDeclined  => "txn-cut",
        TransactionType.FaOfferRejected    => "txn-cut",
        TransactionType.DraftPickMade      => "txn-signed",
        _                                  => ""
    };

    public static string FormatMoney(decimal value)
    {
        var abs  = Math.Abs(value);
        var sign = value < 0 ? "-" : "";
        if (abs >= 1_000_000m) return $"{sign}${abs / 1_000_000m:F1}M";
        if (abs >= 1_000m)     return $"{sign}${abs / 1_000m:F0}K";
        return value.ToString("C0");
    }

    public static string RelativeTime(DateTime utc)
    {
        var diff = DateTime.UtcNow - utc;
        if (diff.TotalSeconds < 60) return "just now";
        if (diff.TotalMinutes < 60) return $"{(int)diff.TotalMinutes}m ago";
        if (diff.TotalHours   < 24) return $"{(int)diff.TotalHours}h ago";
        if (diff.TotalDays    < 7)  return $"{(int)diff.TotalDays}d ago";
        return utc.ToString("MMM d");
    }
}
