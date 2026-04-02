namespace KrediAl.Application.DTOs;

public class ContinueOptionDto
{
    public bool CanContinue { get; set; }
    public string Message { get; set; } = string.Empty;
    public int DaysRemaining { get; set; }
    public bool CommissionPaid { get; set; }
    public decimal? CommissionAmount { get; set; }
    public bool IsExpired { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public string CurrentStatus { get; set; } = string.Empty;
}
