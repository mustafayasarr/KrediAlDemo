namespace KrediAl.Application.DTOs;

public class RefundCommissionResponse
{
    public bool Success { get; set; }
    public decimal RefundAmount { get; set; }
    public string RefundReference { get; set; } = string.Empty;
    public DateTime RefundDate { get; set; }
    public string Message { get; set; } = string.Empty;
}
