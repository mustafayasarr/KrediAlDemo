using KrediAl.Application.Interfaces;
using KrediAl.Domain.Enums;
using KrediAl.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace KrediAl.Infrastructure.Services;

public class MarketplaceService : IMarketplaceService
{
    private readonly KrediAlDbContext _context;
    private readonly ILogger<MarketplaceService> _logger;

    public MarketplaceService(KrediAlDbContext context, ILogger<MarketplaceService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<bool> NotifyOrderStatusAsync(Guid transactionId, MarketplaceOrderStatus status)
    {
        var transaction = await _context.Transactions
            .Include(t => t.Marketplace)
            .FirstOrDefaultAsync(t => t.Id == transactionId);

        if (transaction == null)
        {
            return false;
        }

        _logger.LogInformation(
            "Notifying marketplace {MarketplaceName} about order {OrderId} status: {Status}",
            transaction.Marketplace.Name,
            transaction.OrderId,
            status);

        return true;
    }

    public async Task<bool> NotifyCompletionAsync(Guid transactionId)
    {
        var transaction = await _context.Transactions
            .Include(t => t.Marketplace)
            .FirstOrDefaultAsync(t => t.Id == transactionId);

        if (transaction == null)
        {
            return false;
        }

        _logger.LogInformation(
            "Notifying marketplace {MarketplaceName} about order {OrderId} completion",
            transaction.Marketplace.Name,
            transaction.OrderId);

        return true;
    }

    public async Task<bool> NotifyCancellationAsync(Guid transactionId)
    {
        var transaction = await _context.Transactions
            .Include(t => t.Marketplace)
            .FirstOrDefaultAsync(t => t.Id == transactionId);

        if (transaction == null)
        {
            return false;
        }

        _logger.LogInformation(
            "Notifying marketplace {MarketplaceName} about order {OrderId} cancellation",
            transaction.Marketplace.Name,
            transaction.OrderId);

        return true;
    }

    public async Task<bool> NotifyCancellationAsync(Guid transactionId, string reason)
    {
        var transaction = await _context.Transactions
            .Include(t => t.Marketplace)
            .FirstOrDefaultAsync(t => t.Id == transactionId);

        if (transaction == null)
        {
            return false;
        }

        _logger.LogInformation(
            "Notifying marketplace {MarketplaceName} about order {OrderId} cancellation. Reason: {Reason}",
            transaction.Marketplace.Name,
            transaction.OrderId,
            reason);

        return true;
    }
}
