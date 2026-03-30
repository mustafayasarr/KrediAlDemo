namespace KrediAl.Domain.Entities;

public class Bank
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string ApiUrl { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public decimal MinLoanAmount { get; set; }
    public decimal MaxLoanAmount { get; set; }
    public int MinInstallment { get; set; }
    public int MaxInstallment { get; set; }
    public DateTime CreatedAt { get; set; }
    
    public ICollection<BankOffer> BankOffers { get; set; } = new List<BankOffer>();
}
