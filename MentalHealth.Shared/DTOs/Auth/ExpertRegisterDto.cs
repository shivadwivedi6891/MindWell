namespace MentalHealth.Shared.DTOs.Auth;

public class ExpertRegisterDto
{
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string DisplayName { get; set; } = null!;

    public string Qualification { get; set; } = null!;
    public int ExperienceYears { get; set; }
    public string Specialization { get; set; } = null!;
}
