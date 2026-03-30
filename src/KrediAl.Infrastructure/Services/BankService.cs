using KrediAl.Application.DTOs;
using KrediAl.Application.Interfaces;
using KrediAl.Domain.Entities;
using KrediAl.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace KrediAl.Infrastructure.Services;

public class BankService : IBankService
{
    private readonly KrediAlDbContext _context;

    public BankService(KrediAlDbContext context)
    {
        _context = context;
    }

    public async Task<List<BankOfferDto>> GetOffersAsync(Guid transactionId, decimal amount)
    {
        // Önce mevcut teklifleri kontrol et
        var existingOffers = await _context.BankOffers
            .Include(o => o.Bank)
            .Where(o => o.TransactionId == transactionId && o.ExpiresAt > DateTime.UtcNow)
            .ToListAsync();

        if (existingOffers.Any())
        {
            return existingOffers.Select(o => new BankOfferDto
            {
                Id = o.Id,
                BankName = o.Bank.Name,
                BankCode = o.Bank.Code,
                LoanAmount = o.LoanAmount,
                InstallmentCount = o.InstallmentCount,
                InterestRate = o.InterestRate,
                MonthlyPayment = o.MonthlyPayment,
                TotalPayment = o.TotalPayment,
                ExpiresAt = o.ExpiresAt
            }).ToList();
        }

        // Mevcut teklif yoksa yeni oluştur
        var banks = await _context.Banks
            .Where(b => b.IsActive && b.MinLoanAmount <= amount && b.MaxLoanAmount >= amount)
            .ToListAsync();

        var offers = new List<BankOfferDto>();

        foreach (var bank in banks)
        {
            var installmentOptions = new[] { 12, 24, 36 };
            
            foreach (var installment in installmentOptions)
            {
                if (installment < bank.MinInstallment || installment > bank.MaxInstallment)
                    continue;

                var interestRate = CalculateInterestRate(bank.Code, installment);
                var monthlyPayment = CalculateMonthlyPayment(amount, interestRate, installment);
                var totalPayment = monthlyPayment * installment;

                var offer = new BankOffer
                {
                    Id = Guid.NewGuid(),
                    TransactionId = transactionId,
                    BankId = bank.Id,
                    LoanAmount = amount,
                    InstallmentCount = installment,
                    InterestRate = interestRate,
                    MonthlyPayment = monthlyPayment,
                    TotalPayment = totalPayment,
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddHours(24)
                };

                _context.BankOffers.Add(offer);

                offers.Add(new BankOfferDto
                {
                    Id = offer.Id,
                    BankName = bank.Name,
                    BankCode = bank.Code,
                    LoanAmount = amount,
                    InstallmentCount = installment,
                    InterestRate = interestRate,
                    MonthlyPayment = monthlyPayment,
                    TotalPayment = totalPayment,
                    ExpiresAt = offer.ExpiresAt
                });
            }
        }

        await _context.SaveChangesAsync();

        return offers;
    }

    public async Task<string> GetRedirectUrlAsync(Guid bankId, Guid transactionId)
    {
        var bank = await _context.Banks.FindAsync(bankId);
        if (bank == null)
        {
            throw new InvalidOperationException("Bank not found");
        }

        return $"{bank.ApiUrl}/credit-application?transactionId={transactionId}";
    }

    public async Task<bool> NotifyCreditApprovalAsync(Guid transactionId, string bankReference)
    {
        await Task.Delay(100);
        return true;
    }

    private decimal CalculateInterestRate(string bankCode, int installment)
    {
        var baseRate = bankCode switch
        {
            "GARANTI" => 1.89m,
            "YAPIKREDI" => 1.79m,
            "ISBANK" => 1.99m,
            _ => 2.00m
        };

        var installmentMultiplier = installment switch
        {
            12 => 1.0m,
            24 => 1.1m,
            36 => 1.2m,
            _ => 1.0m
        };

        return baseRate * installmentMultiplier;
    }

    private decimal CalculateMonthlyPayment(decimal principal, decimal monthlyRate, int installments)
    {
        var rate = monthlyRate / 100;
        var payment = principal * (rate * (decimal)Math.Pow((double)(1 + rate), installments)) / 
                      ((decimal)Math.Pow((double)(1 + rate), installments) - 1);
        return Math.Round(payment, 2);
    }
}
