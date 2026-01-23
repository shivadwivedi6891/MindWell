namespace MentalHealth.Shared.DTOs.Auth;

public class ExpertApprovalResponseDto
{
    public Guid ExpertProfileId { get; set; }
    public string ExpertName { get; set; } = null!;
    public string Qualification { get; set; } = null!;
    public int ExperienceYears { get; set; }
    public string Specialization { get; set; } = null!;
    public bool IsApproved { get; set; }
}
