using System;

namespace KrediAl.Domain.Entities;

public class CommissionCalculation
{
    public Guid Id { get; set; }
    public Guid TransactionId { get; set; }
    
    public decimal BaseCommission { get; set; } // %1-2 arası
    public decimal TimeMultiplier { get; set; } // Süre bazlı çarpan
    public decimal GuaranteeFee { get; set; }   // Kefalet ücreti
    public decimal TotalCommission { get; set; }
    public decimal ProcessingFee { get; set; }   // İşlem ücreti
    public decimal TaxRate { get; set; } = 0.18m; // KDV oranı
    public decimal TotalAmountWithTax { get; set; }
    
    public DateTime CalculatedAt { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime? RefundedAt { get; set; }
    
    public bool IsCommissionValidForCurrentTime(TimeSpan timeElapsed)
    {
        // Süre/oran ilişkisi kontrolü
        var maxMultiplier = GetMaxMultiplierForTime(timeElapsed);
        return TimeMultiplier <= maxMultiplier;
    }
    
    private decimal GetMaxMultiplierForTime(TimeSpan timeElapsed)
    {
        // Zaman geçtikçe komisyon oranı artar
        var hours = timeElapsed.TotalHours;
        
        return hours switch
        {
            <= 6 => 1.0m,    // İlk 6 saat: normal
            <= 12 => 1.2m,   // 6-12 saat: %20 fazla
            <= 18 => 1.5m,   // 12-18 saat: %50 fazla
            <= 24 => 2.0m,   // 18-24 saat: %100 fazla
            _ => 0m          // 24+ saat: geçersiz
        };
    }
    
    public static CommissionCalculation Calculate(decimal loanAmount, TimeSpan timeElapsed)
    {
        var calculation = new CommissionCalculation
        {
            Id = Guid.NewGuid(),
            CalculatedAt = DateTime.UtcNow,
            BaseCommission = loanAmount * 0.01m, // %1 baz komisyon
            ProcessingFee = 50m, // Sabit işlem ücreti
            TimeMultiplier = new CommissionCalculation().GetMaxMultiplierForTime(timeElapsed),
            GuaranteeFee = loanAmount * 0.005m // %0.5 kefalet ücreti
        };
        
        calculation.TotalCommission = calculation.BaseCommission * calculation.TimeMultiplier;
        var subtotal = calculation.TotalCommission + calculation.ProcessingFee + calculation.GuaranteeFee;
        calculation.TotalAmountWithTax = subtotal * (1 + calculation.TaxRate);
        
        return calculation;
    }
}
