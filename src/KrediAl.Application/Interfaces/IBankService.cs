using KrediAl.Application.DTOs;

namespace KrediAl.Application.Interfaces;

public interface IBankService
{
    Task<List<BankOfferDto>> GetOffersAsync(Guid transactionId, decimal amount);
    Task<string> GetRedirectUrlAsync(Guid bankId, Guid transactionId);
    Task<bool> NotifyCreditApprovalAsync(Guid transactionId, string bankReference);
}
