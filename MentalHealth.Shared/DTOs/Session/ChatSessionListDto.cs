namespace MentalHealth.Shared.DTOs.Session;
public class ChatSessionListDto
{
    public Guid SessionId { get; set; }
    public string SessionType { get; set; }  = null! ;// "Peer" or "Expert"

    public Guid OtherUserId { get; set; }
    public string OtherUserDisplayName { get; set; } = null!;
    public bool OtherUserIsAnonymous { get; set; }

    public DateTime CreatedAt { get; set; }

    public bool IsActive { get; set; }
    public bool IsPaused { get; set; }
    public DateTime? IsEnded { get; set; }

    public string LastMessage { get; set; } = null!;
    public DateTime? LastMessageAt { get; set; }

    public int UnreadCount { get; set; }
};
