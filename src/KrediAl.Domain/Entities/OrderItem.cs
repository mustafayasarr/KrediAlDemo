namespace KrediAl.Domain.Entities;

public class OrderItem
{
    public Guid Id { get; set; }
    public Guid TransactionId { get; set; }
    public string Category { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public decimal Tax { get; set; }
    public int Quantity { get; set; } = 1;
    
    public Transaction Transaction { get; set; } = null!;
}
