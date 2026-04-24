namespace DeadMoney.Core.Enums;

public enum TransactionType
{
    Cut               = 0,
    Signed            = 1,
    Extended          = 2,
    Traded            = 3,
    PickGranted       = 4,
    PickForfeited     = 5,
    ContractModified  = 6,
    CapAdjusted       = 7,
    ExtensionOffered  = 8,
    ExtensionAgreed   = 9,
    ExtensionCountered = 10,
    ExtensionDeclined = 11,
    FaOfferRejected   = 12
}
