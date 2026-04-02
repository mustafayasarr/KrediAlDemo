using KrediAl.Application.DTOs;

namespace KrediAl.Application.Interfaces;

public interface ITransactionService
{
    Task<CreateSessionResponse> CreateSessionAsync(CreateSessionRequest request);
    Task<TransactionDetailDto> GetTransactionAsync(Guid transactionId);
    Task<bool> ConfirmOrderAsync(Guid transactionId);
    Task<bool> LinkUserToTransactionAsync(Guid transactionId, Guid userId);
    Task<bool> ContinueTransactionAsync(Guid transactionId, Guid userId);
    Task<bool> CancelTransactionAsync(Guid transactionId, Guid userId);
    Task<bool> UpdateFindeksApprovalAsync(Guid transactionId);
    Task<List<BankOfferDto>> GetBankOffersAsync(Guid transactionId);
    Task<bool> SelectBankOfferAsync(Guid transactionId, Guid offerId, Guid userId);
    Task<string> GetBankRedirectUrlAsync(Guid transactionId, Guid offerId);
    Task<bool> CompleteCreditProcessAsync(Guid transactionId, string bankReference);
    
    // Yeni metodlar
    Task<OrderSummaryDto> GetOrderSummaryAsync(Guid transactionId);
    Task<bool> CancelTransactionWithReasonAsync(Guid transactionId, Guid userId, CancelTransactionRequest request);
    Task<ContinueOptionDto> GetContinueOptionAsync(Guid transactionId, Guid userId);
    Task<RefundCommissionResponse> RefundCommissionAsync(Guid transactionId, Guid userId);
}
