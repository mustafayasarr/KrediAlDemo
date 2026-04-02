using System;

namespace KrediAl.Domain.Entities;

public class TransactionTimeline
{
    public Guid Id { get; set; }
    public Guid TransactionId { get; set; }
    
    public DateTime OrderConfirmedAt { get; set; }
    public DateTime? UserAuthenticatedAt { get; set; }
    public DateTime? FindeksApprovedAt { get; set; }
    public DateTime? CommissionPaidAt { get; set; }
    public DateTime? BankSelectionExpiresAt { get; set; }
    public DateTime? UserCanReturnUntil { get; set; } // 3 gün ek süre
    public DateTime? CompletedAt { get; set; }
    public DateTime? CancelledAt { get; set; }
    
    public bool IsBankSelectionExpired => 
        BankSelectionExpiresAt.HasValue && BankSelectionExpiresAt.Value < DateTime.UtcNow;
    
    public bool IsUserReturnWindowActive => 
        UserCanReturnUntil.HasValue && UserCanReturnUntil.Value > DateTime.UtcNow;
    
    public bool IsCommissionValidForCurrentTime(TimeSpan timeElapsed)
    {
        // Süre/oran ilişkisi kontrolü - 24 saate kadar geçerli
        return timeElapsed.TotalHours <= 24;
    }
    
    public TimeSpan GetTimeSinceCommissionPaid()
    {
        if (!CommissionPaidAt.HasValue)
            return TimeSpan.Zero;
            
        return DateTime.UtcNow - CommissionPaidAt.Value;
    }
    
    public static TransactionTimeline CreateForTransaction(Guid transactionId)
    {
        var now = DateTime.UtcNow;
        return new TransactionTimeline
        {
            Id = Guid.NewGuid(),
            TransactionId = transactionId,
            OrderConfirmedAt = now,
            BankSelectionExpiresAt = now.AddHours(24), // 24 saat banka seçim süresi
            UserCanReturnUntil = now.AddDays(3) // 3 gün geri dönüş süresi
        };
    }
}
