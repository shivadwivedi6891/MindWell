namespace MentalHealth.Repository.Entities;

public class GroupSession : BaseEntity
{
    public Guid ChatSessionId { get; set; }
    public Guid TopicId { get; set; }

     public GroupTopic Topic { get; set; } = null!;

    public ChatSession ChatSession { get; set; } = null!;
}
