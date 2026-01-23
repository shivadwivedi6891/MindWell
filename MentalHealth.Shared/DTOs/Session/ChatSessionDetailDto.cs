namespace MentalHealth.Shared.DTOs.Session;
public class ChatSessionDetailDto
{
    public Guid SessionId { get; set; }
    public string SessionType { get; set; } = null!;

    public bool IsActive { get; set; }
    public bool IsPaused { get; set; }
    public DateTime? IsEnded { get; set; }

    public DateTime? CreatedAt { get; set; }

    public List<ChatParticipantDto> Participants { get; set; } = new();

    public bool CanPause { get; set; }
    public bool CanResume { get; set; }
    public bool CanEnd { get; set; }
}
