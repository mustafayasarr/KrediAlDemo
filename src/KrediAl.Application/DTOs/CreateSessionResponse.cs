using Swashbuckle.AspNetCore.Annotations;

namespace KrediAl.Application.DTOs;

public class CreateSessionResponse
{
    [SwaggerSchema(Description = "Pazaryerindeki sipariş numarası")]
    public string OrderId { get; set; } = string.Empty;
    
    [SwaggerSchema(Description = "Müşterinin yönlendirileceği URL. Bu URL'de müşteri giriş yapıp kredi başvurusunu tamamlayacak.")]
    public string RedirectUrl { get; set; } = string.Empty;
    
    [SwaggerSchema(Description = "Transaction ID")]
    public Guid TransactionId { get; set; }
}
