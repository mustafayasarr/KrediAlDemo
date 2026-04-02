using KrediAl.Domain.Enums;

namespace KrediAl.Application.Interfaces;

public interface IMarketplaceService
{
    Task<bool> NotifyOrderStatusAsync(Guid transactionId, MarketplaceOrderStatus status);
    Task<bool> NotifyCompletionAsync(Guid transactionId);
    Task<bool> NotifyCancellationAsync(Guid transactionId);
    Task<bool> NotifyCancellationAsync(Guid transactionId, string reason);
}
