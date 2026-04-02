using KrediAl.Application.DTOs;
using KrediAl.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace KrediAl.API.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
[Produces("application/json")]
[SwaggerTag("Authentication")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    [HttpPost("register")]
    [SwaggerOperation(
        Summary = "👤 Müşteri: Yeni hesap kaydı oluştur",
        Description = @"**Kullanım Senaryosu:** Müşteri pazaryerinden yönlendirildikten sonra, kredi başvurusunu tamamlayabilmek için sisteme kayıt olur.

**Ne Yapar?**
1. Müşteri bilgilerini doğrular (email, TC, telefon)
2. Şifreyi hash'leyerek güvenli şekilde saklar
3. JWT token döner (24 saat geçerli)

**Örnek Akış:**
1. Müşteri pazaryerinde 'Kredi ile Öde' seçer
2. Pazaryeri session oluşturur ve müşteriyi Kredi Al'a yönlendirir
3. Müşteri bu endpoint'i çağırarak hesap oluşturur
4. Müşteri token alarak diğer işlemleri yapabilir

**Gerekli Bilgiler:**
- Email (giriş için kullanılacak)
- Şifre (minimum 6 karakter)
- Ad, Soyad, Telefon, TC Kimlik No

**ÖNEMLİ:** Bu endpoint pazaryeri kimlik bilgileriyle DEĞİL, müşterinin kendi bilgileriyle çalışır!",
        OperationId = "Register",
        Tags = new[] { "Authentication" }
    )]
    [SwaggerResponse(200, "Kayıt başarılı, JWT token döner", typeof(ApiResponse<AuthResponse>))]
    [SwaggerResponse(400, "Geçersiz istek veya email zaten kullanımda", typeof(ApiResponse))]
    [SwaggerResponse(500, "Sunucu hatası", typeof(ApiResponse))]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Register([FromBody] UserRegistrationRequest request)
    {
        try
        {
            var response = await _authService.RegisterAsync(request);
            return Ok(ApiResponse<AuthResponse>.SuccessResponse(response, "Kayıt başarılı"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration");
            return StatusCode(500, ApiResponse.ErrorResponse("Kayıt sırasında hata oluştu"));
        }
    }

    [HttpPost("login")]
    [SwaggerOperation(
        Summary = "🔐 Müşteri: Hesaba giriş yap",
        Description = @"**Kullanım Senaryosu:** Kayıtlı müşterinin kredi başvurusuna devam etmek için sisteme giriş yapması.

**Ne Yapar?**
1. Email ve şifreyi doğrular
2. JWT token döner (24 saat geçerli)
3. Token ile diğer korumalı endpoint'lere erişim sağlar

**Örnek Akış:**
1. Müşteri Kredi Al sayfasına gelir
2. 'Giriş Yap' seçeneğini tıklar
3. Email ve şifresini girer
4. Bu endpoint'i çağırır
5. Token alarak kredi başvurusuna devam eder

**Token Kullanımı:**
- Swagger'da 'Authorize' butonuna tıklayın
- 'Bearer {token}' formatında girin
- Tüm korumalı endpoint'ler kullanılabilir

**Token Süresi:** 24 saat
- Süre dolunca yeniden giriş yapılmalı
- 401 hatası alırsanız token expire olmuştur",
        OperationId = "Login",
        Tags = new[] { "Authentication" }
    )]
    [SwaggerResponse(200, "Giriş başarılı, JWT token döner", typeof(ApiResponse<AuthResponse>))]
    [SwaggerResponse(401, "Geçersiz email veya şifre", typeof(ApiResponse))]
    [SwaggerResponse(500, "Sunucu hatası", typeof(ApiResponse))]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Login([FromBody] UserLoginRequest request)
    {
        try
        {
            var response = await _authService.LoginAsync(request);
            return Ok(ApiResponse<AuthResponse>.SuccessResponse(response, "Giriş başarılı"));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ApiResponse.ErrorResponse("Geçersiz email veya şifre"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login");
            return StatusCode(500, ApiResponse.ErrorResponse("Giriş sırasında hata oluştu"));
        }
    }
}
