using KrediAl.Application.DTOs;
using KrediAl.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;

namespace KrediAl.API.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
[Produces("application/json")]
[SwaggerTag("Transaction")]
public class TransactionController : ControllerBase
{
    private readonly ITransactionService _transactionService;
    private readonly IFindeksService _findeksService;
    private readonly IPaymentService _paymentService;
    private readonly ILogger<TransactionController> _logger;

    public TransactionController(
        ITransactionService transactionService,
        IFindeksService findeksService,
        IPaymentService paymentService,
        ILogger<TransactionController> logger)
    {
        _transactionService = transactionService;
        _findeksService = findeksService;
        _paymentService = paymentService;
        _logger = logger;
    }

    [HttpGet("{transactionId}")]
    [SwaggerOperation(
        Summary = "📋 Transaction detaylarını getir",
        Description = @"**Kullanım Senaryosu:** Müşterinin veya sistemin transaction durumunu ve detaylarını görüntülemesi.

**Ne Yapar?**
1. Transaction ID ile ilgili işlemi bulur
2. Tüm detayları döner (durum, müşteri, sipariş, ödeme)
3. İşlem akışının neresinde olduğunu gösterir

**Örnek Akış:**
1. Müşteri Kredi Al sayfasına gelir
2. Transaction ID URL'den alınır
3. Bu endpoint çağrılarak işlem detayları gösterilir
4. Müşteri bir sonraki adımı görür

**Dönen Bilgiler:**
- Transaction durumu (Created, Confirmed, Pending, etc.)
- Müşteri bilgileri (eğer bağlı ise)
- Sipariş detayları
- Banka teklifleri (varsa)
- Ödeme durumu

**Kim Çağırabilir?**
- Herkes (authentication gerekmez)",
        OperationId = "GetTransaction",
        Tags = new[] { "Transaction" }
    )]
    [SwaggerResponse(200, "Transaction detayları", typeof(ApiResponse<TransactionDetailDto>))]
    [SwaggerResponse(404, "Transaction bulunamadı", typeof(ApiResponse))]
    public async Task<ActionResult<ApiResponse<TransactionDetailDto>>> GetTransaction(Guid transactionId)
    {
        try
        {
            var transaction = await _transactionService.GetTransactionAsync(transactionId);
            return Ok(ApiResponse<TransactionDetailDto>.SuccessResponse(transaction, "Transaction detayları başarıyla getirildi"));
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ApiResponse.ErrorResponse("Transaction bulunamadı", new List<string> { ex.Message }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting transaction");
            return StatusCode(500, ApiResponse.ErrorResponse("Transaction getirilirken hata oluştu", new List<string> { ex.Message }));
        }
    }

    [HttpPost("{transactionId}/confirm-order")]
    [SwaggerOperation(
        Summary = "✅ Siparişi onayla",
        Description = @"**Kullanım Senaryosu:** Müşterinin kredi başvurusuna başlamadan önce siparişi onaylaması.

**Ne Yapar?**
1. Transaction durumunu 'Confirmed' yapar
2. Müşterinin başvuruya devam etmesini sağlar
3. Sipariş bilgilerini doğrular

**Örnek Akış:**
1. Müşteri Kredi Al sayfasına gelir
2. Transaction detayları gösterilir
3. Müşteri 'Siparişi Onayla' butonuna tıklar
4. Bu endpoint çağrılır
5. Müşteri giriş/kayıt sayfasına yönlendirilir

**Kim Çağırabilir?**
- Herkes (authentication gerekmez)",
        OperationId = "ConfirmOrder",
        Tags = new[] { "Transaction" }
    )]
    [SwaggerResponse(200, "Sipariş onaylandı", typeof(ApiResponse))]
    [SwaggerResponse(400, "Sipariş onaylanamadı", typeof(ApiResponse))]
    public async Task<ActionResult<ApiResponse>> ConfirmOrder(Guid transactionId)
    {
        try
        {
            var result = await _transactionService.ConfirmOrderAsync(transactionId);
            if (result)
            {
                return Ok(ApiResponse.SuccessResponse("Sipariş başarıyla onaylandı"));
            }
            return BadRequest(ApiResponse.ErrorResponse("Sipariş onaylanamadı"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error confirming order");
            return StatusCode(500, ApiResponse.ErrorResponse("Sipariş onaylanırken hata oluştu", new List<string> { ex.Message }));
        }
    }

    [Authorize]
    [HttpPost("{transactionId}/link-user")]
    [SwaggerOperation(
        Summary = "🔗 Müşteriyi transaction'a bağla",
        Description = @"**Kullanım Senaryosu:** Giriş yapan müşterinin transaction ile ilişkilendirilmesi.

**Ne Yapar?**
1. JWT token'dan kullanıcı ID'sini alır
2. Kullanıcıyı transaction'a bağlar
3. Transaction durumunu 'Confirmed' yapar
4. Müşterinin başvuruya devam etmesini sağlar

**Örnek Akış:**
1. Müşteri giriş/kayıt yapar
2. JWT token alır
3. Bu endpoint çağrılır
4. Müşteri transaction'a bağlanır
5. Findeks onayı sayfasına yönlendirilir

**Kim Çağırabilir?**
- Sadece giriş yapmış müşteriler (JWT token gerekli)",
        OperationId = "LinkUser",
        Tags = new[] { "Transaction" }
    )]
    [SwaggerResponse(200, "Kullanıcı başarıyla bağlandı", typeof(ApiResponse))]
    [SwaggerResponse(400, "Kullanıcı bağlanamadı", typeof(ApiResponse))]
    [SwaggerResponse(401, "Yetkisiz erişim", typeof(ApiResponse))]
    public async Task<ActionResult<ApiResponse>> LinkUser(Guid transactionId)
    {
        try
        {
            var userId = GetUserId();
            var result = await _transactionService.LinkUserToTransactionAsync(transactionId, userId);
            if (result)
            {
                return Ok(ApiResponse.SuccessResponse("Kullanıcı başarıyla bağlandı"));
            }
            return BadRequest(ApiResponse.ErrorResponse("Kullanıcı bağlanamadı"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error linking user");
            return StatusCode(500, ApiResponse.ErrorResponse("Kullanıcı bağlanırken hata oluştu", new List<string> { ex.Message }));
        }
    }

    [Authorize]
    [HttpPost("{transactionId}/continue")]
    [SwaggerOperation(
        Summary = "⏭️ İşleme devam et",
        Description = @"**Kullanım Senaryosu:** Müşterinin kredi başvurusunda bir sonraki adıma geçmesi.

**Ne Yapar?**
1. Müşterinin Findeks onayı durumunu kontrol eder
2. Findeks onayı varsa banka tekliflerine geçiş sağlar
3. Findeks onayı yoksa Findeks onayı istemesi gerektiğini belirtir

**Örnek Akış:**
1. Müşteri transaction'a bağlanır
2. Bu endpoint çağrılır
3. Findeks onayı kontrol edilir
4. Onay varsa: Banka teklifleri sayfasına yönlendirilir
5. Onay yoksa: Findeks onayı sayfasına yönlendirilir

**Dönen Bilgiler:**
- canContinue: false, requiresFindeks: true (Findeks gerekli)
- canContinue: true (Banka tekliflerine geçilebilir)

**Kim Çağırabilir?**
- Sadece giriş yapmış müşteriler (JWT token gerekli)",
        OperationId = "ContinueTransaction",
        Tags = new[] { "Transaction" }
    )]
    [SwaggerResponse(200, "İşlem durumu kontrol edildi", typeof(ApiResponse))]
    [SwaggerResponse(401, "Yetkisiz erişim", typeof(ApiResponse))]
    public async Task<ActionResult<ApiResponse>> ContinueTransaction(Guid transactionId)
    {
        try
        {
            var userId = GetUserId();
            var result = await _transactionService.ContinueTransactionAsync(transactionId, userId);
            if (result)
            {
                return Ok(new { success = true, message = "İşleme devam edilebilir", data = new { canContinue = true, requiresFindeks = false } });
            }
            return Ok(new { success = true, message = "Findeks onayı gerekiyor", data = new { canContinue = false, requiresFindeks = true } });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error continuing transaction");
            return StatusCode(500, ApiResponse.ErrorResponse("İşleme devam edilirken hata oluştu", new List<string> { ex.Message }));
        }
    }

    [Authorize]
    [HttpPost("{transactionId}/cancel")]
    [SwaggerOperation(
        Summary = "❌ İşlemi iptal et",
        Description = @"**Kullanım Senaryosu:** Müşterinin kredi başvurusunu iptal etmesi.

**Ne Yapar?**
1. Transaction durumunu 'Cancelled' yapar
2. Pazaryerinin RejectUrl'ine bildirim gönderir
3. Müşteriye iptal onayı gösterir

**Örnek Akış:**
1. Müşteri kredi başvurusu sırasında 'İptal Et' butonuna tıklar
2. Bu endpoint çağrılır
3. Transaction iptal edilir
4. Pazaryerinin RejectUrl'ine POST gönderilir
5. Müşteri pazaryerine geri yönlendirilir

**İptal Sebepleri:**
- Müşteri kendi isteğiyle iptal edebilir
- Belirli bir süre içinde işlem tamamlanmazsa otomatik iptal
- Findeks reddi durumunda

**Kim Çağırabilir?**
- Sadece giriş yapmış müşteriler (JWT token gerekli)",
        OperationId = "CancelTransaction",
        Tags = new[] { "Transaction" }
    )]
    [SwaggerResponse(200, "İşlem başarıyla iptal edildi", typeof(ApiResponse))]
    [SwaggerResponse(400, "İşlem iptal edilemedi", typeof(ApiResponse))]
    [SwaggerResponse(401, "Yetkisiz erişim", typeof(ApiResponse))]
    public async Task<ActionResult<ApiResponse>> CancelTransaction(Guid transactionId)
    {
        try
        {
            var userId = GetUserId();
            var result = await _transactionService.CancelTransactionAsync(transactionId, userId);
            if (result)
            {
                return Ok(ApiResponse.SuccessResponse("İşlem başarıyla iptal edildi"));
            }
            return BadRequest(ApiResponse.ErrorResponse("İşlem iptal edilemedi"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling transaction");
            return StatusCode(500, ApiResponse.ErrorResponse("İşlem iptal edilirken hata oluştu", new List<string> { ex.Message }));
        }
    }

    [Authorize]
    [HttpPost("{transactionId}/request-findeks")]
    [SwaggerOperation(
        Summary = "📊 Findeks onayı iste",
        Description = @"**Kullanım Senaryosu:** Müşterinin Findeks kredi notu onayı alması.

**Ne Yapar?**
1. Müşterinin Findeks kaydını kontrol eder
2. Kredi notunu hesaplar
3. Kefalet tutarını belirler
4. Onayı kaydeder

**Örnek Akış:**
1. Müşteri Findeks onayı sayfasına gelir
2. 'Findeks Onayı Al' butonuna tıklar
3. Bu endpoint çağrılır
4. Findeks onayı alınır
5. Banka teklifleri sayfasına yönlendirilir

**Kim Çağırabilir?**
- Sadece giriş yapmış müşteriler (JWT token gerekli)",
        OperationId = "RequestFindeks",
        Tags = new[] { "Transaction" }
    )]
    [SwaggerResponse(200, "Findeks onayı alındı", typeof(ApiResponse))]
    [SwaggerResponse(400, "Findeks onayı alınamadı", typeof(ApiResponse))]
    [SwaggerResponse(401, "Yetkisiz erişim", typeof(ApiResponse))]
    public async Task<ActionResult<ApiResponse>> RequestFindeks(Guid transactionId)
    {
        try
        {
            var userId = GetUserId();
            if (userId == Guid.Empty)
            {
                return BadRequest(ApiResponse.ErrorResponse("Kullanıcı bulunamadı"));
            }
            
            var result = await _findeksService.RequestApprovalAsync(userId);
            if (result)
            {
                await _findeksService.ProcessPaymentAsync(userId);
                // Transaction durumunu güncelle
                await _transactionService.UpdateFindeksApprovalAsync(transactionId);
                return Ok(ApiResponse.SuccessResponse("Findeks onayı başarıyla alındı"));
            }
            return BadRequest(ApiResponse.ErrorResponse("Findeks onayı alınamadı"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error requesting Findeks");
            return StatusCode(500, ApiResponse.ErrorResponse("Findeks onayı istenirken hata oluştu", new List<string> { ex.Message }));
        }
    }

    [Authorize]
    [HttpGet("{transactionId}/bank-offers")]
    [SwaggerOperation(
        Summary = "🏦 Banka tekliflerini getir",
        Description = @"**Kullanım Senaryosu:** Müşterinin uygun banka tekliflerini görmesi.

**Ne Yapar?**
1. Müşterinin kredi notunu kontrol eder
2. Uygun bankaları belirler
3. Faiz oranlarını ve taksit seçeneklerini hesaplar
4. Teklif listesi döner

**Örnek Akış:**
1. Müşteri Findeks onayı alır
2. Banka teklifleri sayfasına gelir
3. Bu endpoint çağrılır
4. Tüm teklifler gösterilir
5. Müşteri bir teklif seçer

**Dönen Bilgiler:**
- Banka adı
- Faiz oranı
- Taksit seçenekleri
- Aylık ödeme
- Toplam geri ödeme

**Kim Çağırabilir?**
- Sadece giriş yapmış müşteriler (JWT token gerekli)",
        OperationId = "GetBankOffers",
        Tags = new[] { "Transaction" }
    )]
    [SwaggerResponse(200, "Banka teklifleri", typeof(ApiResponse<List<BankOfferDto>>))]
    [SwaggerResponse(401, "Yetkisiz erişim", typeof(ApiResponse))]
    public async Task<ActionResult<ApiResponse<List<BankOfferDto>>>> GetBankOffers(Guid transactionId)
    {
        try
        {
            var offers = await _transactionService.GetBankOffersAsync(transactionId);
            return Ok(ApiResponse<List<BankOfferDto>>.SuccessResponse(offers, "Banka teklifleri başarıyla getirildi"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting bank offers");
            return StatusCode(500, ApiResponse.ErrorResponse("Banka teklifleri getirilirken hata oluştu", new List<string> { ex.Message }));
        }
    }

    [Authorize]
    [HttpPost("{transactionId}/pay-commission")]
    [SwaggerOperation(
        Summary = "💰 Komisyon öde",
        Description = @"**Kullanım Senaryosu:** Müşterinin kredi kefalet komisyonunu ödemesi.

**Ne Yapar?**
1. Komisyon tutarını hesaplar (%3)
2. Ödeme işlemini simüle eder
3. Transaction durumunu günceller
4. Banka seçimi için hazırlık yapar

**Örnek Akış:**
1. Müşteri banka tekliflerini görür
2. Bir teklif seçmeden önce komisyon öder
3. 'Komisyon Öde' butonuna tıklar
4. Bu endpoint çağrılır
5. Ödeme başarılı olursa banka seçimi aktif hale gelir

**Komisyon:**
- Kredi tutarının %3'ü
- Kefalet hizmeti için
- Geri iade edilebilir

**Kim Çağırabilir?**
- Sadece giriş yapmış müşteriler (JWT token gerekli)",
        OperationId = "PayCommission",
        Tags = new[] { "Transaction" }
    )]
    [SwaggerResponse(200, "Komisyon ödendi", typeof(ApiResponse))]
    [SwaggerResponse(400, "Komisyon ödenemedi", typeof(ApiResponse))]
    [SwaggerResponse(401, "Yetkisiz erişim", typeof(ApiResponse))]
    public async Task<ActionResult<ApiResponse>> PayCommission(Guid transactionId)
    {
        try
        {
            var userId = GetUserId();
            var result = await _paymentService.ProcessCommissionPaymentAsync(transactionId, userId);
            if (result)
            {
                return Ok(ApiResponse.SuccessResponse("Komisyon başarıyla ödendi"));
            }
            return BadRequest(ApiResponse.ErrorResponse("Komisyon ödenemedi"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing commission payment");
            return StatusCode(500, ApiResponse.ErrorResponse("Komisyon ödenirken hata oluştu", new List<string> { ex.Message }));
        }
    }

    [Authorize]
    [HttpPost("{transactionId}/select-offer/{offerId}")]
    [SwaggerOperation(
        Summary = "🎯 Banka teklifi seç",
        Description = @"**Kullanım Senaryosu:** Müşterinin kredi için bir banka teklifi seçmesi.

**Ne Yapar?**
1. Seçilen banka teklifini kaydeder
2. Transaction durumunu 'BankSelected' yapar
3. Bankanın yönlendirme URL'ini döner
4. Müşteriyi bankaya yönlendirir

**Örnek Akış:**
1. Müşteri komisyon öder
2. Banka tekliflerini görür
3. Bir teklif seçer
4. 'Bu Teklifi Seç' butonuna tıklar
5. Bu endpoint çağrılır
6. Müşteri bankaya yönlendirilir
7. Bankada kredi başvurusunu tamamlar

**Sonraki Adımlar:**
- Banka krediyi onaylar
- Banka /api/marketplace/credit-completed çağırır
- Pazaryerinin SuccessUrl'ine bildirim gider

**Kim Çağırabilir?**
- Sadece giriş yapmış müşteriler (JWT token gerekli)",
        OperationId = "SelectBankOffer",
        Tags = new[] { "Transaction" }
    )]
    [SwaggerResponse(200, "Banka teklifi seçildi", typeof(ApiResponse))]
    [SwaggerResponse(400, "Banka teklifi seçilemedi", typeof(ApiResponse))]
    [SwaggerResponse(401, "Yetkisiz erişim", typeof(ApiResponse))]
    public async Task<ActionResult<ApiResponse>> SelectBankOffer(Guid transactionId, Guid offerId)
    {
        try
        {
            var userId = GetUserId();
            var result = await _transactionService.SelectBankOfferAsync(transactionId, offerId, userId);
            if (result)
            {
                var redirectUrl = await _transactionService.GetBankRedirectUrlAsync(transactionId, offerId);
                return Ok(new { success = true, message = "Banka teklifi başarıyla seçildi", data = new { redirectUrl = redirectUrl } });
            }
            return BadRequest(ApiResponse.ErrorResponse("Banka teklifi seçilemedi"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error selecting bank offer");
            return StatusCode(500, ApiResponse.ErrorResponse("Banka teklifi seçilirken hata oluştu", new List<string> { ex.Message }));
        }
    }

    [HttpGet("{transactionId}/order-summary")]
    [SwaggerOperation(
        Summary = "📋 Sipariş özetini getir",
        Description = @"**Kullanım Senaryosu:** Müşteriye sipariş detayları ve komisyon bilgilerini gösterme.

**Ne Yapar?**
1. Transaction'a ait sipariş detaylarını getirir
2. Komisyon tutarını hesaplar
3. Kalan süreyi gösterir

**Örnek Akış:**
1. Müşteri işleme başlamadan önce özet gösterilir
2. Toplam tutar, komisyon ve ürün detayları görüntülenir

**Kim Çağırabilir?**
- Herkes (authentication gerekmez)",
        OperationId = "GetOrderSummary",
        Tags = new[] { "Transaction" }
    )]
    [SwaggerResponse(200, "Sipariş özeti", typeof(ApiResponse<OrderSummaryDto>))]
    [SwaggerResponse(404, "Transaction bulunamadı", typeof(ApiResponse))]
    public async Task<ActionResult<ApiResponse<OrderSummaryDto>>> GetOrderSummary(Guid transactionId)
    {
        try
        {
            var summary = await _transactionService.GetOrderSummaryAsync(transactionId);
            return Ok(ApiResponse<OrderSummaryDto>.SuccessResponse(summary, "Sipariş özeti başarıyla getirildi"));
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ApiResponse.ErrorResponse("Transaction bulunamadı", new List<string> { ex.Message }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting order summary");
            return StatusCode(500, ApiResponse.ErrorResponse("Sipariş özeti getirilirken hata oluştu", new List<string> { ex.Message }));
        }
    }

    [HttpPost("{transactionId}/cancel-with-reason")]
    [Authorize]
    [SwaggerOperation(
        Summary = "❌ İşlemi iptal et (nedeni ile)",
        Description = @"**Kullanım Senaryosu:** Müşterinin işlemi iptal etmesi ve neden belirtmesi.

**Ne Yapar?**
1. İşlemi iptal eder
2. Komisyon ödenmişse iade işlemi başlatır
3. Pazaryerine iptal bildirimi gönderir

**Örnek Akış:**
1. Müşteri 'İptal Et' butonuna tıklar
2. İptal nedeni seçer/yazar
3. Onay verir
4. İşlem iptal edilir ve komisyon iade edilir

**Kim Çağırabilir?**
- Sadece işlem sahibi kullanıcı (authentication gerekir)",
        OperationId = "CancelTransactionWithReason",
        Tags = new[] { "Transaction" }
    )]
    [SwaggerResponse(200, "İşlem iptal edildi", typeof(ApiResponse))]
    [SwaggerResponse(400, "İşlem iptal edilemedi", typeof(ApiResponse))]
    [SwaggerResponse(401, "Yetkisiz erişim", typeof(ApiResponse))]
    public async Task<ActionResult<ApiResponse>> CancelTransactionWithReason(Guid transactionId, [FromBody] CancelTransactionRequest request)
    {
        try
        {
            var userId = GetUserId();
            var result = await _transactionService.CancelTransactionWithReasonAsync(transactionId, userId, request);
            if (result)
            {
                return Ok(ApiResponse.SuccessResponse("İşlem başarıyla iptal edildi"));
            }
            return BadRequest(ApiResponse.ErrorResponse("İşlem iptal edilemedi"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling transaction");
            return StatusCode(500, ApiResponse.ErrorResponse("İşlem iptal edilirken hata oluştu", new List<string> { ex.Message }));
        }
    }

    [HttpGet("{transactionId}/continue-option")]
    [Authorize]
    [SwaggerOperation(
        Summary = "🔄 İşleme devam seçeneğini kontrol et",
        Description = @"**Kullanım Senaryosu:** Müşterinin 3 gün içinde geri dönüp işleme devam edip edemeyeceğini kontrol etme.

**Ne Yapar?**
1. Komisyon ödenip ödenmediğini kontrol eder
2. Sürenin dolup dolmadığını kontrol eder
3. Kalan gün sayısını hesaplar
4. Devam edebilir mi bilgisini döner

**Örnek Akış:**
1. Müşteri komisyon ödedikten sonra çıkış yapar
2. 2 gün sonra geri gelir
3. Bu endpoint çağrılır
4. 'Devam edebilirsiniz, 1 gün süreniz kaldı' mesajı gösterilir

**Kim Çağırabilir?**
- Sadece işlem sahibi kullanıcı (authentication gerekir)",
        OperationId = "GetContinueOption",
        Tags = new[] { "Transaction" }
    )]
    [SwaggerResponse(200, "Devam seçeneği bilgisi", typeof(ApiResponse<ContinueOptionDto>))]
    [SwaggerResponse(404, "Transaction bulunamadı", typeof(ApiResponse))]
    [SwaggerResponse(401, "Yetkisiz erişim", typeof(ApiResponse))]
    public async Task<ActionResult<ApiResponse<ContinueOptionDto>>> GetContinueOption(Guid transactionId)
    {
        try
        {
            var userId = GetUserId();
            var option = await _transactionService.GetContinueOptionAsync(transactionId, userId);
            return Ok(ApiResponse<ContinueOptionDto>.SuccessResponse(option, "Devam seçeneği bilgisi getirildi"));
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ApiResponse.ErrorResponse("Transaction bulunamadı", new List<string> { ex.Message }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting continue option");
            return StatusCode(500, ApiResponse.ErrorResponse("Devam seçeneği kontrol edilirken hata oluştu", new List<string> { ex.Message }));
        }
    }

    [HttpPost("{transactionId}/refund-commission")]
    [Authorize]
    [SwaggerOperation(
        Summary = "💰 Komisyon iadesini başlat",
        Description = @"**Kullanım Senaryosu:** Müşterinin ödediği komisyonun iade edilmesi.

**Ne Yapar?**
1. Komisyon ödemesini kontrol eder
2. İade işlemini başlatır
3. İade referans numarası oluşturur
4. Payment durumunu 'Refunded' yapar

**Örnek Akış:**
1. Müşteri işlemi iptal eder
2. Bu endpoint çağrılır
3. Komisyon iade edilir
4. İade bilgileri döner

**Kim Çağırabilir?**
- Sadece işlem sahibi kullanıcı (authentication gerekir)",
        OperationId = "RefundCommission",
        Tags = new[] { "Transaction" }
    )]
    [SwaggerResponse(200, "Komisyon iade edildi", typeof(ApiResponse<RefundCommissionResponse>))]
    [SwaggerResponse(400, "İade işlemi başarısız", typeof(ApiResponse))]
    [SwaggerResponse(401, "Yetkisiz erişim", typeof(ApiResponse))]
    public async Task<ActionResult<ApiResponse<RefundCommissionResponse>>> RefundCommission(Guid transactionId)
    {
        try
        {
            var userId = GetUserId();
            var response = await _transactionService.RefundCommissionAsync(transactionId, userId);
            if (response.Success)
            {
                return Ok(ApiResponse<RefundCommissionResponse>.SuccessResponse(response, "Komisyon başarıyla iade edildi"));
            }
            return BadRequest(ApiResponse<RefundCommissionResponse>.ErrorResponse("Komisyon iadesi başarısız oldu"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refunding commission");
            return StatusCode(500, ApiResponse.ErrorResponse("Komisyon iadesi sırasında hata oluştu", new List<string> { ex.Message }));
        }
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("Invalid user ID");
        }
        return userId;
    }
}
