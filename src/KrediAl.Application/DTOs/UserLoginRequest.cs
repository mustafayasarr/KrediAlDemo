using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;

namespace KrediAl.Application.DTOs;

public class UserLoginRequest
{
    [Required]
    [EmailAddress]
    [SwaggerSchema(Description = "Email adresi")]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    [SwaggerSchema(Description = "Şifre")]
    public string Password { get; set; } = string.Empty;
}
