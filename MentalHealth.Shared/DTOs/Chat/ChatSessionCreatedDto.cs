namespace MentalHealth.Shared.DTOs.Chat;

public class ChatSessionCreatedDto
{
    public Guid ChatSessionId { get; set; }
    public Guid ExpertId { get; set; }
    public Guid UserId { get; set; }
    public DateTime CreatedAt { get; set; }
}