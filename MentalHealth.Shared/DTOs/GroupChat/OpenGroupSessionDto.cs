namespace MentalHealth.Shared.DTOs.GroupChat;

public class OpenGroupSessionDto
{
    public Guid TopicId { get; set; }
    public bool IsActive { get; set; } = true;
}