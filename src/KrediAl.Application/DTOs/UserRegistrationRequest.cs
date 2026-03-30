using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;

namespace KrediAl.Application.DTOs;

public class UserRegistrationRequest
{
    [Required]
    [EmailAddress]
    [SwaggerSchema(Description = "Müşteri email adresi (giriş için kullanılacak)")]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    [MinLength(6)]
    [SwaggerSchema(Description = "Şifre (minimum 6 karakter)")]
    public string Password { get; set; } = string.Empty;
    
    [Required]
    [SwaggerSchema(Description = "Ad")]
    public string FirstName { get; set; } = string.Empty;
    
    [Required]
    [SwaggerSchema(Description = "Soyad")]
    public string LastName { get; set; } = string.Empty;
    
    [Required]
    [Phone]
    [SwaggerSchema(Description = "Telefon numarası")]
    public string PhoneNumber { get; set; } = string.Empty;
    
    [Required]
    [StringLength(11, MinimumLength = 11)]
    [SwaggerSchema(Description = "TC Kimlik No (11 haneli)")]
    public string NationalId { get; set; } = string.Empty;
}
