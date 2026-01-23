namespace MentalHealth.Shared.DTOs.Chat;

public class ChatMessageDto
{
    public Guid ChatSessionId { get; set; }
    public Guid SenderId { get; set; }
    public string Content { get; set; } = null!;
    public DateTime SentAt { get; set; }
    public bool IsAnonymous { get; set; }
}
