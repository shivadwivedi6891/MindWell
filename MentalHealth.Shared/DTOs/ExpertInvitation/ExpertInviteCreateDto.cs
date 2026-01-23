namespace MentalHealth.Shared.DTOs.ExpertInvitation;

public class ExpertInviteCreateDto
{
    public Guid UserId { get; set; }
    public string Message { get; set; } = null!;
}
