namespace MentalHealth.Shared.DTOs.ExpertInvitation;

public class LowMoodUserDto
{
    public Guid UserId { get; set; }
    public string Period { get; set; } = null!;
    public double AverageScore { get; set; }
}
