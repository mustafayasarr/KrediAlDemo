namespace KrediAl.Application.Interfaces;

public interface IFindeksService
{
    Task<bool> CheckApprovalAsync(Guid userId);
    Task<bool> RequestApprovalAsync(Guid userId);
    Task<bool> ProcessPaymentAsync(Guid userId);
    Task<decimal> CalculateGuaranteeAmountAsync(Guid userId, decimal loanAmount);
}
