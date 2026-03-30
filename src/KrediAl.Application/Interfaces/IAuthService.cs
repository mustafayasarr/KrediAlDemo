using KrediAl.Application.DTOs;

namespace KrediAl.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(UserRegistrationRequest request);
    Task<AuthResponse> LoginAsync(UserLoginRequest request);
    Task<bool> ValidateMarketplaceCredentialsAsync(string username, string password);
}
