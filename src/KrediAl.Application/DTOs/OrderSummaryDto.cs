namespace KrediAl.Application.DTOs;

public class OrderSummaryDto
{
    public Guid TransactionId { get; set; }
    public string OrderId { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public decimal CommissionAmount { get; set; }
    public decimal CommissionRate { get; set; }
    public List<OrderItemDto> Items { get; set; } = new();
    public string MarketplaceName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public int DaysToExpire { get; set; }
}
