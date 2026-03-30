namespace KrediAl.Application.Interfaces;

public interface IPaymentService
{
    Task<decimal> CalculateCommissionAsync(decimal loanAmount);
    Task<bool> ProcessCommissionPaymentAsync(Guid transactionId, Guid userId);
    Task<bool> RefundCommissionAsync(Guid transactionId);
    Task<bool> ValidateCommissionPaymentAsync(Guid transactionId);
}
