namespace MentalHealth.Repository.Entities;
public class GroupTopic : BaseEntity
{
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public bool IsActive { get; set; }
}
