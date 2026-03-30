using KrediAl.Domain.Enums;

namespace KrediAl.Application.DTOs;

public class TransactionDetailDto
{
    public Guid Id { get; set; }
    public string OrderId { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public TransactionStatus Status { get; set; }
    public List<OrderItemDto> Items { get; set; } = new();
    public bool CanContinue { get; set; }
    public bool IsExpired { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public bool RequiresFindeks { get; set; }
}

public class OrderItemDto
{
    public string Category { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public decimal Tax { get; set; }
    public int Quantity { get; set; }
}
