namespace MentalHealth.Shared.DTOs.ExpertInvitation;
public class ExpertSentInvitationDto
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }
    public string UserDisplayName { get; set; } = null!;

    public string Message { get; set; } = null!;
    public string Status { get; set; } = null!;   // Pending / Accepted / Declined

    public DateTime CreatedAt { get; set; }
}
