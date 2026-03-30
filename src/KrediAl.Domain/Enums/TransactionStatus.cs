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
    BankRedirected,
    Completed,
    Rejected,
    Cancelled,
    Expired
}
