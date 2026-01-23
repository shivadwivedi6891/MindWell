namespace MentalHealth.Shared.DTOs.ExpertInvitation;

public class ExpertInvitationResponseDto
{
    public Guid InvitationId { get; set; }
    public string ExpertName { get; set; } = null!;

    public string Message { get; set; } = null!;
    public string Status { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
}
