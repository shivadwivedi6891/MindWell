namespace MentalHealth.Repository.Entities;

public class Report : BaseEntity
{
    public Guid ChatSessionId { get; set; }
    public Guid ReportedByUserId { get; set; }
    public string Reason { get; set; } = null!;
    public string Status { get; set; } = null!;

     public ChatSession ChatSession { get; set; } = null!;
}
