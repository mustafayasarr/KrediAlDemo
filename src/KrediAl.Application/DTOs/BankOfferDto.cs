namespace KrediAl.Application.DTOs;

public class BankOfferDto
{
    public Guid Id { get; set; }
    public string BankName { get; set; } = string.Empty;
    public string BankCode { get; set; } = string.Empty;
    public decimal LoanAmount { get; set; }
    public int InstallmentCount { get; set; }
    public decimal InterestRate { get; set; }
    public decimal MonthlyPayment { get; set; }
    public decimal TotalPayment { get; set; }
    public DateTime ExpiresAt { get; set; }
}
