using KrediAl.Domain.Enums;

namespace KrediAl.Domain.Entities;

public class Payment
{
    public Guid Id { get; set; }
    public Guid TransactionId { get; set; }
    public decimal CommissionAmount { get; set; }
    public decimal CommissionRate { get; set; }
    public decimal Amount { get; set; }
    public PaymentStatus Status { get; set; }
    public string? PaymentReference { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime? RefundedAt { get; set; }
    public DateTime? RefundDate { get; set; }
    
    public Transaction Transaction { get; set; } = null!;
}
