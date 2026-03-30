using Swashbuckle.AspNetCore.Annotations;

namespace KrediAl.Application.DTOs;

public class AuthResponse
{
    [SwaggerSchema(Description = "Kullanıcı ID")]
    public Guid UserId { get; set; }
    
    [SwaggerSchema(Description = "Email adresi")]
    public string Email { get; set; } = string.Empty;
    
    [SwaggerSchema(Description = "JWT Bearer token - Diğer endpoint'lerde Authorization header'ında kullanılır")]
    public string Token { get; set; } = string.Empty;
    
    [SwaggerSchema(Description = "Ad")]
    public string FirstName { get; set; } = string.Empty;
    
    [SwaggerSchema(Description = "Soyad")]
    public string LastName { get; set; } = string.Empty;
}
