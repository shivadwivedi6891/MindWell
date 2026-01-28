namespace MentalHealth.Shared.DTOs.GroupChat;

public class GroupRoomDto
{
    public Guid ChatSessionId { get; set; }
    public Guid TopicId { get; set; }
    public string TopicName { get; set; }
    public string Description { get; set; }
    public int ParticipantCount { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}