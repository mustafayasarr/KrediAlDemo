using KrediAl.Application.Interfaces;
using KrediAl.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace KrediAl.Infrastructure.Services;

public class FindeksService : IFindeksService
{
    private readonly KrediAlDbContext _context;

    public FindeksService(KrediAlDbContext context)
    {
        _context = context;
    }

    public async Task<bool> CheckApprovalAsync(Guid userId)
    {
        var user = await _context.Users.FindAsync(userId);
        return user?.FindeksApprovalGranted ?? false;
    }

    public async Task<bool> RequestApprovalAsync(Guid userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return false;

        user.FindeksApprovalGranted = true;
        user.FindeksApprovalDate = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ProcessPaymentAsync(Guid userId)
    {
        await Task.Delay(100);
        return true;
    }

    public async Task<decimal> CalculateGuaranteeAmountAsync(Guid userId, decimal loanAmount)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return 0;

        var guaranteeRate = 0.15m;
        var calculatedAmount = loanAmount * guaranteeRate;

        user.GuaranteeAmount = calculatedAmount;
        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return calculatedAmount;
    }
}
