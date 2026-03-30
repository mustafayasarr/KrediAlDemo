using KrediAl.Domain.Enums;

namespace KrediAl.Domain.Entities;

public class Transaction
{
    public Guid Id { get; set; }
    public Guid MarketplaceId { get; set; }
    public Guid? UserId { get; set; }
    public string OrderId { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public TransactionStatus Status { get; set; }
    public string SuccessUrl { get; set; } = string.Empty;
    public string RejectUrl { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public bool IsExpired => ExpiresAt.HasValue && ExpiresAt.Value < DateTime.UtcNow;
    
    public Marketplace Marketplace { get; set; } = null!;
    public User? User { get; set; }
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    public ICollection<BankOffer> BankOffers { get; set; } = new List<BankOffer>();
    public Payment? Payment { get; set; }
}
