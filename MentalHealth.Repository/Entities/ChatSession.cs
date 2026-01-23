namespace MentalHealth.Repository.Entities;

public class ChatSession : BaseEntity
{
    public string SessionType { get; set; } = null!;
    public bool IsAnonymous { get; set; }
    public string? UserAlias { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }

     public bool IsActive { get; set; }
    public bool IsPaused { get; set; }

    public ICollection<ChatParticipant> Participants { get; set; } = new List<ChatParticipant>();
    public ICollection<Message> Messages { get; set; } = new List<Message>();
    public ICollection<ChatReflection> Reflections { get; set; } = new List<ChatReflection>();
}
