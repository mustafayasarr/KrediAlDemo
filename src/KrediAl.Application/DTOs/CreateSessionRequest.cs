using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;

namespace KrediAl.Application.DTOs;

public class CreateSessionRequest
{
    [Required]
    [SwaggerSchema(Description = "Pazaryerine özel kullanıcı adı (ClientID benzeri)")]
    public string MpUser { get; set; } = string.Empty;
    
    [Required]
    [SwaggerSchema(Description = "Pazaryerine özel şifre (ClientSecret benzeri)")]
    public string MpPassword { get; set; } = string.Empty;
    
    [Required]
    [SwaggerSchema("Sipariş bilgileri")]
    public OrderRequest Order { get; set; } = null!;
}

public class OrderRequest
{
    [Required]
    [Url]
    [SwaggerSchema(Description = "Kredi onaylandığında pazaryerinin yönlendirileceği URL. Transaction tamamlandığında bu URL'e POST isteği gönderilir.")]
    public string SuccessUrl { get; set; } = string.Empty;
    
    [Required]
    [Url]
    [SwaggerSchema(Description = "Kredi reddedildiğinde veya iptal edildiğinde pazaryerinin yönlendirileceği URL. Transaction iptal/red durumunda bu URL'e POST isteği gönderilir.")]
    public string RejectUrl { get; set; } = string.Empty;
    
    [Required]
    [SwaggerSchema(Description = "Pazaryerindeki sipariş numarası")]
    public string OrderId { get; set; } = string.Empty;
    
    [Required]
    [Range(100, 1000000)]
    [SwaggerSchema(Description = "Toplam sipariş tutarı (TL)")]
    public decimal TotalAmount { get; set; }
    
    [Required]
    [MinLength(1)]
    [SwaggerSchema(Description = "Sipariş kalemleri listesi")]
    public List<OrderItemRequest> Items { get; set; } = new();
}

public class OrderItemRequest
{
    [Required]
    [SwaggerSchema(Description = "Ürün kategorisi")]
    public string Category { get; set; } = string.Empty;
    
    [Required]
    [Range(0.01, 1000000)]
    [SwaggerSchema(Description = "Birim fiyat (TL)")]
    public decimal UnitPrice { get; set; }
    
    [Required]
    [Range(0, 100)]
    [SwaggerSchema(Description = "KDV oranı (%)")]
    public decimal Tax { get; set; }
}
