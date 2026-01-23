namespace MentalHealth.Repository.Entities;

public class ChatReflection : BaseEntity
{
    public Guid ChatSessionId { get; set; }
    public Guid UserId { get; set; }

    public int MoodAfterScore { get; set; }
    public string? Feedback { get; set; }

    public ChatSession ChatSession { get; set; } = null!;
    public User User { get; set; } = null!;
}
