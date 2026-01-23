namespace MentalHealth.Repository.Entities;

public class ChatParticipant 
{
 public Guid Id { get; set; }
    public Guid ChatSessionId { get; set; }
    public Guid UserId { get; set; }

    public string Role { get; set; } = null!;
    public DateTime JoinedAt { get; set; }

 public bool IsIdentityRevealed { get; set; }
    public ChatSession ChatSession { get; set; } = null!;
    public User User { get; set; } = null!;
}
