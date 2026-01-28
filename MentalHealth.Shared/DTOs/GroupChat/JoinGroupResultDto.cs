namespace MentalHealth.Shared.DTOs.GroupChat;

public class JoinGroupResultDto
{
    public Guid ChatSessionId { get; set; }
    public string AnonymousName { get; set; }
    public int JoinPosition { get; set; }
    public bool Success { get; set; }
    public string Message { get; set; }
}