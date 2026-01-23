namespace MentalHealth.Repository.Entities;

public class ExpertProfile : BaseEntity
{
    public Guid UserId { get; set; }
    public string Qualification { get; set; } = null!;
    public int ExperienceYears { get; set; }
    public string Specialization { get; set; } = null!;

    public bool IsApproved { get; set; } = false;
    public DateTime? ApprovedAt { get; set; }
    public Guid? ApprovedByAdminId { get; set; }

    public User User { get; set; } = null!;
}
