using KrediAl.Application.DTOs;
using KrediAl.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace KrediAl.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[SwaggerTag("Marketplace")]
public class MarketplaceController : ControllerBase
{
    private readonly ITransactionService _transactionService;
    private readonly ILogger<MarketplaceController> _logger;

    public MarketplaceController(ITransactionService transactionService, ILogger<MarketplaceController> logger)
    {
        _transactionService = transactionService;
        _logger = logger;
    }

    [HttpPost("create-session")]
    [SwaggerOperation(
        Summary = "🏪 Pazaryeri: Yeni kredi başvuru session'ı oluştur",
        Description = @"**Kullanım Senaryosu:** Müşteri pazaryerinde alışveriş yapıp kredi seçtiğinde, pazaryeri bu endpoint'i çağırarak müşteri için bir kredi başvuru session'ı başlatır.

**Ne Yapar?**
1. Pazaryeri kimlik bilgilerini doğrular (mp_user & mp_password)
2. Yeni bir transaction oluşturur
3. Müşterinin yönlendirileceği URL döner

**Örnek Akış:**
1. Müşteri pazaryerinde 'Kredi ile Öde' butonuna tıklar
2. Pazaryeri bu endpoint'i çağırır
3. Müşteri dönen redirectUrl'e yönlendirilir
4. Müşteri orada kredi başvurusunu tamamlar

**Demo Bilgileri:**
- mp_user: 'demo_mp'
- mp_password: 'demo123'",
        OperationId = "CreateSession",
        Tags = new[] { "Marketplace" }
    )]
    [SwaggerResponse(200, "Session başarıyla oluşturuldu", typeof(ApiResponse<CreateSessionResponse>))]
    [SwaggerResponse(401, "Geçersiz pazaryeri kimlik bilgileri", typeof(ApiResponse))]
    [SwaggerResponse(400, "Geçersiz istek", typeof(ApiResponse<CreateSessionResponse>))]
    public async Task<ActionResult<ApiResponse<CreateSessionResponse>>> CreateSession([FromBody] CreateSessionRequest request)
    {
        try
        {
            var response = await _transactionService.CreateSessionAsync(request);
            return Ok(ApiResponse<CreateSessionResponse>.SuccessResponse(response, "Session başarıyla oluşturuldu"));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ApiResponse.ErrorResponse("Geçersiz pazaryeri kimlik bilgileri", new List<string> { ex.Message }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating session");
            return BadRequest(ApiResponse.ErrorResponse("Session oluşturulurken hata oluştu", new List<string> { ex.Message }));
        }
    }

    [HttpPost("credit-completed")]
    [SwaggerOperation(
        Summary = "🏦 Banka: Kredi onaylandı bildirimi",
        Description = @"**Kullanım Senaryosu:** Banka müşterinin kredi başvurusunu onayladıktan sonra, bu endpoint'i çağırarak Kredi Al sistemini bilgilendirir.

**Ne Yapar?**
1. Krediyi 'Completed' durumuna getirir
2. Pazaryerinin SuccessUrl'ine bildirim gönderir
3. Müşteriye kredi onayı bilgisi gösterilir

**Örnek Akış:**
1. Müşteri banka seçer ve bankaya yönlendirilir
2. Müşteri bankada kredi başvurusunu tamamlar
3. Banka krediyi onaylar
4. Banka bu endpoint'i çağırır
5. Kredi Al pazaryerinin SuccessUrl'ine POST gönderir
6. Pazaryeri müşteriyi başarı sayfasına yönlendirir

**Kim Çağırır?**
- Banka API'leri (simülasyon)
- Test amaçlı manuel çağrı",
        OperationId = "CreditCompleted",
        Tags = new[] { "Marketplace" }
    )]
    [SwaggerResponse(200, "Kredi işlemi başarıyla tamamlandı", typeof(ApiResponse))]
    [SwaggerResponse(400, "İşlem tamamlanamadı", typeof(ApiResponse))]
    public async Task<ActionResult<ApiResponse>> CreditCompleted([FromBody] CreditCompletedRequest request)
    {
        try
        {
            var result = await _transactionService.CompleteCreditProcessAsync(request.TransactionId, request.BankReference);
            if (result)
            {
                return Ok(ApiResponse.SuccessResponse("Kredi işlemi başarıyla tamamlandı"));
            }
            return BadRequest(ApiResponse.ErrorResponse("Kredi işlemi tamamlanamadı"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing credit process");
            return BadRequest(ApiResponse.ErrorResponse("İşlem sırasında hata oluştu", new List<string> { ex.Message }));
        }
    }
}

public class CreditCompletedRequest
{
    [SwaggerSchema("Transaction ID")]
    public Guid TransactionId { get; set; }
    
    [SwaggerSchema("Banka referans numarası")]
    public string BankReference { get; set; } = string.Empty;
}
