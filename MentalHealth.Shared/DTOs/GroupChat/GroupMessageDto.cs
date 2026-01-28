namespace MentalHealth.Shared.DTOs.GroupChat;

public class GroupMessageDto
{
    public Guid MessageId { get; set; }
    public Guid ChatSessionId { get; set; }
    public string SenderAnonymousName { get; set; }
    public string Content { get; set; }
    public DateTime SentAt { get; set; }
}