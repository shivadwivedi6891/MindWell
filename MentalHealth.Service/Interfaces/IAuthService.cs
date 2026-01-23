using MentalHealth.Shared.DTOs.Auth;

namespace MentalHealth.Service.Interfaces;

public interface IAuthService
{
    Task RegisterAsync(RegisterRequestDto request);
    Task<LoginResponseDto> LoginAsync(LoginRequestDto request);

    Task<LoginResponseDto> ToggleAnonymousAsync(Guid userId);

}
