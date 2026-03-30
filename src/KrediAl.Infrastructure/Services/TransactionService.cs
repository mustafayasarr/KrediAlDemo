using KrediAl.Application.DTOs;
using KrediAl.Application.Interfaces;
using KrediAl.Domain.Entities;
using KrediAl.Domain.Enums;
using KrediAl.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace KrediAl.Infrastructure.Services;

public class TransactionService : ITransactionService
{
    private readonly KrediAlDbContext _context;
    private readonly IAuthService _authService;
    private readonly IMarketplaceService _marketplaceService;
    private readonly IFindeksService _findeksService;
    private readonly IBankService _bankService;
    private readonly IPaymentService _paymentService;
    private readonly IConfiguration _configuration;

    public TransactionService(
        KrediAlDbContext context,
        IAuthService authService,
        IMarketplaceService marketplaceService,
        IFindeksService findeksService,
        IBankService bankService,
        IPaymentService paymentService,
        IConfiguration configuration)
    {
        _context = context;
        _authService = authService;
        _marketplaceService = marketplaceService;
        _findeksService = findeksService;
        _bankService = bankService;
        _paymentService = paymentService;
        _configuration = configuration;
    }

    public async Task<CreateSessionResponse> CreateSessionAsync(CreateSessionRequest request)
    {
        if (!await _authService.ValidateMarketplaceCredentialsAsync(request.MpUser, request.MpPassword))
        {
            throw new UnauthorizedAccessException("Invalid marketplace credentials");
        }

        var marketplace = await _context.Marketplaces.FirstOrDefaultAsync(m => m.Username == request.MpUser);
        if (marketplace == null)
        {
            throw new InvalidOperationException("Marketplace not found");
        }

        var transaction = new Transaction
        {
            Id = Guid.NewGuid(),
            MarketplaceId = marketplace.Id,
            OrderId = request.Order.OrderId,
            TotalAmount = request.Order.TotalAmount,
            Status = TransactionStatus.Created,
            SuccessUrl = request.Order.SuccessUrl,
            RejectUrl = request.Order.RejectUrl,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(3)
        };

        foreach (var item in request.Order.Items)
        {
            transaction.OrderItems.Add(new OrderItem
            {
                Id = Guid.NewGuid(),
                TransactionId = transaction.Id,
                Category = item.Category,
                UnitPrice = item.UnitPrice,
                Tax = item.Tax
            });
        }

        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();

        var baseUrl = _configuration["AppSettings:BaseUrl"] ?? "http://localhost:5800";
        var redirectUrl = $"{baseUrl}/transaction.html#{transaction.Id}";

        return new CreateSessionResponse
        {
            OrderId = request.Order.OrderId,
            RedirectUrl = redirectUrl,
            TransactionId = transaction.Id
        };
    }

    public async Task<TransactionDetailDto> GetTransactionAsync(Guid transactionId)
    {
        var transaction = await _context.Transactions
            .Include(t => t.OrderItems)
            .FirstOrDefaultAsync(t => t.Id == transactionId);

        if (transaction == null)
        {
            throw new InvalidOperationException("Transaction not found");
        }

        return new TransactionDetailDto
        {
            Id = transaction.Id,
            OrderId = transaction.OrderId,
            TotalAmount = transaction.TotalAmount,
            Status = transaction.Status,
            Items = transaction.OrderItems.Select(i => new OrderItemDto
            {
                Category = i.Category,
                UnitPrice = i.UnitPrice,
                Tax = i.Tax,
                Quantity = i.Quantity
            }).ToList(),
            CanContinue = !transaction.IsExpired && transaction.Status != TransactionStatus.Completed && transaction.Status != TransactionStatus.Cancelled,
            IsExpired = transaction.IsExpired,
            ExpiresAt = transaction.ExpiresAt,
            RequiresFindeks = transaction.Status == TransactionStatus.UserAuthenticated || transaction.Status == TransactionStatus.FindeksApprovalPending
        };
    }

    public async Task<bool> ConfirmOrderAsync(Guid transactionId)
    {
        var transaction = await _context.Transactions.FindAsync(transactionId);
        if (transaction == null) return false;
        
        // Sadece Created durumundan OrderConfirmed'a geçilebilir
        if (transaction.Status != TransactionStatus.Created)
        {
            throw new InvalidOperationException($"Sipariş sadece oluşturulduktan sonra onaylanabilir. Mevcut durum: {transaction.Status}");
        }

        transaction.Status = TransactionStatus.OrderConfirmed;
        transaction.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        await _marketplaceService.NotifyOrderStatusAsync(transactionId, MarketplaceOrderStatus.WaitingPayment);

        return true;
    }

    public async Task<bool> LinkUserToTransactionAsync(Guid transactionId, Guid userId)
    {
        var transaction = await _context.Transactions.FindAsync(transactionId);
        if (transaction == null) return false;
        
        // Sadece OrderConfirmed durumundan UserAuthenticated'a geçilebilir
        if (transaction.Status != TransactionStatus.OrderConfirmed)
        {
            throw new InvalidOperationException($"Kullanıcı bağlama sadece sipariş onaylandıktan sonra yapılabilir. Mevcut durum: {transaction.Status}");
        }
        
        // Aynı kullanıcı zaten bağlanmış mı?
        if (transaction.UserId == userId)
        {
            throw new InvalidOperationException("Bu kullanıcı zaten transaction'a bağlı.");
        }

        transaction.UserId = userId;
        transaction.Status = TransactionStatus.UserAuthenticated;
        transaction.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ContinueTransactionAsync(Guid transactionId, Guid userId)
    {
        var transaction = await _context.Transactions.FindAsync(transactionId);
        if (transaction == null || transaction.UserId != userId) return false;
        
        // UserAuthenticated, FindeksApprovalPending veya FindeksApproved durumlarından devam edilebilir
        if (transaction.Status != TransactionStatus.UserAuthenticated && 
            transaction.Status != TransactionStatus.FindeksApprovalPending &&
            transaction.Status != TransactionStatus.FindeksApproved)
        {
            throw new InvalidOperationException($"İşleme devam etme sadece kullanıcı giriş yaptıktan sonra mümkündür. Mevcut durum: {transaction.Status}");
        }

        if (transaction.IsExpired)
        {
            transaction.Status = TransactionStatus.Expired;
            await _context.SaveChangesAsync();
            return false;
        }

        // Zaten Findeks onayı varsa, tekrar kontrol etmeye gerek yok
        if (transaction.Status == TransactionStatus.FindeksApproved)
        {
            return true;
        }

        var hasFindeksApproval = await _findeksService.CheckApprovalAsync(userId);
        if (!hasFindeksApproval)
        {
            transaction.Status = TransactionStatus.FindeksApprovalPending;
            await _context.SaveChangesAsync();
            return false;
        }

        transaction.Status = TransactionStatus.FindeksApproved;
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> CancelTransactionAsync(Guid transactionId, Guid userId)
    {
        var transaction = await _context.Transactions
            .Include(t => t.Payment)
            .FirstOrDefaultAsync(t => t.Id == transactionId);

        if (transaction == null || transaction.UserId != userId) return false;

        transaction.Status = TransactionStatus.Cancelled;
        transaction.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        if (transaction.Payment != null && transaction.Payment.Status == PaymentStatus.Completed)
        {
            await _paymentService.RefundCommissionAsync(transactionId);
        }

        await _marketplaceService.NotifyCancellationAsync(transactionId);

        return true;
    }

    public async Task<bool> UpdateFindeksApprovalAsync(Guid transactionId)
    {
        var transaction = await _context.Transactions.FindAsync(transactionId);
        if (transaction == null) return false;
        
        // UserAuthenticated veya FindeksApprovalPending durumundan FindeksApproved'a geçilebilir
        if (transaction.Status != TransactionStatus.UserAuthenticated && transaction.Status != TransactionStatus.FindeksApprovalPending)
        {
            if (transaction.Status == TransactionStatus.FindeksApproved)
            {
                throw new InvalidOperationException("Findeks onayı zaten alınmış.");
            }
            throw new InvalidOperationException($"Findeks onayı sadece kullanıcı giriş yaptıktan sonra alınabilir. Mevcut durum: {transaction.Status}");
        }

        transaction.Status = TransactionStatus.FindeksApproved;
        transaction.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<BankOfferDto>> GetBankOffersAsync(Guid transactionId)
    {
        var transaction = await _context.Transactions.FindAsync(transactionId);
        if (transaction == null)
        {
            throw new InvalidOperationException("Transaction not found");
        }

        // Findeks onayı kontrol et
        if (transaction.Status != TransactionStatus.FindeksApproved)
        {
            throw new InvalidOperationException($"Findeks onayı gerekli. Mevcut durum: {transaction.Status}. Banka tekliflerini görmek için önce Findeks onayı almalısınız.");
        }

        var offers = await _bankService.GetOffersAsync(transactionId, transaction.TotalAmount);
        
        return offers;
    }

    public async Task<bool> SelectBankOfferAsync(Guid transactionId, Guid offerId, Guid userId)
    {
        var transaction = await _context.Transactions
            .Include(t => t.BankOffers)
            .Include(t => t.Payment)
            .FirstOrDefaultAsync(t => t.Id == transactionId);

        if (transaction == null || transaction.UserId != userId) return false;
        
        // FindeksApproved veya CommissionPaid durumundan BankRedirected'a geçilebilir
        if (transaction.Status != TransactionStatus.FindeksApproved && transaction.Status != TransactionStatus.CommissionPaid)
        {
            throw new InvalidOperationException($"Banka seçimi sadece Findeks onayı aldıktan sonra yapılabilir. Mevcut durum: {transaction.Status}");
        }

        if (transaction.IsExpired)
        {
            transaction.Status = TransactionStatus.Expired;
            await _context.SaveChangesAsync();
            return false;
        }

        var offer = transaction.BankOffers.FirstOrDefault(o => o.Id == offerId);
        if (offer == null)
        {
            throw new InvalidOperationException("Banka teklifi bulunamadı. Lütfen önce banka tekliflerini görüntüleyin.");
        }
        
        if (offer.ExpiresAt < DateTime.UtcNow)
        {
            throw new InvalidOperationException("Banka teklifi süresi dolmuş.");
        }

        var hasCommissionPayment = transaction.Payment != null && transaction.Payment.Status == PaymentStatus.Completed;
        if (!hasCommissionPayment)
        {
            throw new InvalidOperationException("Komisyon ödemesi yapılmamış. Lütfen önce komisyon ödemesini tamamlayın.");
        }

        var guaranteeAmount = await _findeksService.CalculateGuaranteeAmountAsync(userId, transaction.TotalAmount);
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            throw new InvalidOperationException("Kullanıcı bulunamadı.");
        }
        
        if (user.GuaranteeAmount < guaranteeAmount)
        {
            throw new InvalidOperationException($"Yetersiz kefalet tutarı. Gerekli: {guaranteeAmount:C}, Mevcut: {user.GuaranteeAmount:C}");
        }

        offer.IsSelected = true;
        transaction.Status = TransactionStatus.BankRedirected;
        transaction.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<string> GetBankRedirectUrlAsync(Guid transactionId, Guid offerId)
    {
        var offer = await _context.BankOffers
            .Include(o => o.Bank)
            .FirstOrDefaultAsync(o => o.Id == offerId && o.TransactionId == transactionId);

        if (offer == null)
        {
            throw new InvalidOperationException("Bank offer not found");
        }

        return await _bankService.GetRedirectUrlAsync(offer.BankId, transactionId);
    }

    public async Task<bool> CompleteCreditProcessAsync(Guid transactionId, string bankReference)
    {
        var transaction = await _context.Transactions.FindAsync(transactionId);
        if (transaction == null) return false;

        transaction.Status = TransactionStatus.Completed;
        transaction.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        await _marketplaceService.NotifyCompletionAsync(transactionId);

        return true;
    }
}
