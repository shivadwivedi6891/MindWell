namespace MentalHealth.Shared.DTOs.Admin;

public class CreateUserDto
{
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string DisplayName { get; set; } = null!;
    public string Role { get; set; } = null!; // "User" or "Expert"
    public bool IsActive { get; set; } = true;
    public bool IsAnonymous { get; set; } = false;
}
