namespace MentalHealth.Shared.DTOs.Chat;

public class CreateChatSessionRequest
{
    public Guid ExpertId { get; set; }
    public bool IsAnonymous { get; set; }
}
