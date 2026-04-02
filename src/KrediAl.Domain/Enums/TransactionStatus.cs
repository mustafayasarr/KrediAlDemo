namespace KrediAl.Domain.Enums;

public enum TransactionStatus
{
    Created,
    OrderConfirmed,
    UserAuthenticated,
    FindeksApprovalPending,
    FindeksApproved,
    BankOffersReceived,
    CommissionPaid,
    CommissionRefunded,        // Komisyon iade edildi
    BankRedirected,
    BankSelectionPending,      // Banka seçimi bekleniyor
    BankSelectionExpired,      // Banka seçim süresi doldu
    PendingUserReturn,         // 3 gün içinde geri dönüş bekleniyor
    Completed,
    RejectedByUser,            // Müşteri tarafından reddedildi
    Cancelled,
    Expired
}
