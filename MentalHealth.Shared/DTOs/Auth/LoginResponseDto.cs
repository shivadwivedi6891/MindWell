namespace MentalHealth.Shared.DTOs.Auth;

public class LoginResponseDto
{
    public string Token { get; set; } = default!;
    public string Role { get; set; } = default!;
    public bool IsAnonymous { get; set; }
    public Guid UserId { get; set; }
    public string Email { get; set; } = default!;
}
