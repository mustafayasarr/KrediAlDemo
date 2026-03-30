using KrediAl.Application.Interfaces;
using KrediAl.Domain.Entities;
using KrediAl.Domain.Enums;
using KrediAl.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace KrediAl.Infrastructure.Services;

public class PaymentService : IPaymentService
{
    private readonly KrediAlDbContext _context;
    private const decimal CommissionRate = 0.03m;

    public PaymentService(KrediAlDbContext context)
    {
        _context = context;
    }

    public async Task<decimal> CalculateCommissionAsync(decimal loanAmount)
    {
        return await Task.FromResult(loanAmount * CommissionRate);
    }

    public async Task<bool> ProcessCommissionPaymentAsync(Guid transactionId, Guid userId)
    {
        var transaction = await _context.Transactions
            .Include(t => t.Payment)
            .FirstOrDefaultAsync(t => t.Id == transactionId);

        if (transaction == null || transaction.UserId != userId)
        {
            return false;
        }

        if (transaction.Payment != null && transaction.Payment.Status == PaymentStatus.Completed)
        {
            return true;
        }

        var commissionAmount = await CalculateCommissionAsync(transaction.TotalAmount);

        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            TransactionId = transactionId,
            CommissionAmount = commissionAmount,
            CommissionRate = CommissionRate,
            Status = PaymentStatus.Completed,
            PaymentReference = $"PAY-{Guid.NewGuid().ToString()[..8].ToUpper()}",
            CreatedAt = DateTime.UtcNow,
            PaidAt = DateTime.UtcNow
        };

        _context.Payments.Add(payment);
        transaction.Status = TransactionStatus.CommissionPaid;
        transaction.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RefundCommissionAsync(Guid transactionId)
    {
        var payment = await _context.Payments
            .FirstOrDefaultAsync(p => p.TransactionId == transactionId);

        if (payment == null || payment.Status != PaymentStatus.Completed)
        {
            return false;
        }

        payment.Status = PaymentStatus.Refunded;
        payment.RefundedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ValidateCommissionPaymentAsync(Guid transactionId)
    {
        var payment = await _context.Payments
            .FirstOrDefaultAsync(p => p.TransactionId == transactionId);

        return payment != null && payment.Status == PaymentStatus.Completed;
    }
}
