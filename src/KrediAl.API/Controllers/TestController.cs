using KrediAl.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KrediAl.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    private readonly KrediAlDbContext _context;

    public TestController(KrediAlDbContext context)
    {
        _context = context;
    }

    [HttpGet("marketplace")]
    public async Task<ActionResult> GetMarketplace()
    {
        var marketplace = await _context.Marketplaces.FirstOrDefaultAsync(m => m.Username == "demo_mp");
        
        if (marketplace == null)
        {
            return Ok(new { message = "Marketplace not found" });
        }

        return Ok(new 
        { 
            id = marketplace.Id,
            username = marketplace.Username,
            name = marketplace.Name,
            isActive = marketplace.IsActive,
            passwordHashLength = marketplace.PasswordHash?.Length ?? 0,
            passwordHashStart = marketplace.PasswordHash?.Substring(0, Math.Min(20, marketplace.PasswordHash.Length))
        });
    }

    [HttpGet("banks")]
    public async Task<ActionResult> GetBanks()
    {
        var banks = await _context.Banks.ToListAsync();
        
        return Ok(new 
        { 
            count = banks.Count,
            banks = banks.Select(b => new 
            {
                id = b.Id,
                name = b.Name,
                code = b.Code,
                isActive = b.IsActive,
                minLoanAmount = b.MinLoanAmount,
                maxLoanAmount = b.MaxLoanAmount,
                minInstallment = b.MinInstallment,
                maxInstallment = b.MaxInstallment
            })
        });
    }

    [HttpGet("bank-offers/{transactionId}")]
    public async Task<ActionResult> GetBankOffers(Guid transactionId)
    {
        var offers = await _context.BankOffers
            .Include(o => o.Bank)
            .Where(o => o.TransactionId == transactionId)
            .ToListAsync();
        
        return Ok(new 
        { 
            count = offers.Count,
            offers = offers.Select(o => new 
            {
                id = o.Id,
                bankName = o.Bank.Name,
                bankCode = o.Bank.Code,
                loanAmount = o.LoanAmount,
                installmentCount = o.InstallmentCount,
                interestRate = o.InterestRate,
                monthlyPayment = o.MonthlyPayment,
                totalPayment = o.TotalPayment,
                expiresAt = o.ExpiresAt,
                isSelected = o.IsSelected
            })
        });
    }

    [HttpGet("debug-offers/{transactionId}")]
    public async Task<ActionResult> DebugOffers(Guid transactionId)
    {
        var transaction = await _context.Transactions.FindAsync(transactionId);
        var totalAmount = transaction?.TotalAmount ?? 0;
        var banks = await _context.Banks
            .Where(b => b.IsActive && b.MinLoanAmount <= totalAmount && b.MaxLoanAmount >= totalAmount)
            .ToListAsync();
        
        return Ok(new 
        { 
            transactionId = transactionId,
            transactionExists = transaction != null,
            transactionStatus = transaction?.Status,
            totalAmount = transaction?.TotalAmount,
            eligibleBanksCount = banks.Count,
            eligibleBanks = banks.Select(b => new 
            {
                id = b.Id,
                name = b.Name,
                code = b.Code,
                minLoanAmount = b.MinLoanAmount,
                maxLoanAmount = b.MaxLoanAmount,
                minInstallment = b.MinInstallment,
                maxInstallment = b.MaxInstallment,
                isActive = b.IsActive
            })
        });
    }

    [HttpPost("test-bcrypt")]
    public ActionResult TestBCrypt([FromBody] TestPasswordRequest request)
    {
        var hash = BCrypt.Net.BCrypt.HashPassword(request.Password);
        var isValid = BCrypt.Net.BCrypt.Verify(request.Password, hash);
        var isValidAgainstStored = !string.IsNullOrEmpty(request.StoredHash) 
            ? BCrypt.Net.BCrypt.Verify(request.Password, request.StoredHash) 
            : false;
        
        return Ok(new 
        { 
            generatedHash = hash,
            isValidAgainstGenerated = isValid,
            isValidAgainstStored = isValidAgainstStored
        });
    }
}

public class TestPasswordRequest
{
    public string Password { get; set; } = string.Empty;
    public string? StoredHash { get; set; }
}
