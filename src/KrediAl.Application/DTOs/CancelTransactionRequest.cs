namespace KrediAl.Application.DTOs;

public class CancelTransactionRequest
{
    public string CancelReason { get; set; } = string.Empty;
    public string? AdditionalNotes { get; set; }
}
