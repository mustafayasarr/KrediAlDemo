namespace KrediAl.Domain.Entities;

public class BankOffer
{
    public Guid Id { get; set; }
    public Guid TransactionId { get; set; }
    public Guid BankId { get; set; }
    public decimal LoanAmount { get; set; }
    public int InstallmentCount { get; set; }
    public decimal InterestRate { get; set; }
    public decimal MonthlyPayment { get; set; }
    public decimal TotalPayment { get; set; }
    public bool IsSelected { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    
    public Transaction Transaction { get; set; } = null!;
    public Bank Bank { get; set; } = null!;
}
