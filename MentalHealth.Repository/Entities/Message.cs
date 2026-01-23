namespace MentalHealth.Repository.Entities;

public class Message : BaseEntity
{
    public Guid ChatSessionId { get; set; }
    public Guid SenderId { get; set; }
    public string Content { get; set; } = null!;
    public bool IsSystemMessage { get; set; }

     public bool IsDeleted { get; set; }
     public DateTime SentAt { get; set; }
    public DateTime? EditedAt { get; set; }

    public ChatSession ChatSession { get; set; } = null!;
}
