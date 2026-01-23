namespace MentalHealth.Repository.Entities;

public class MoodQuestion : BaseEntity
{
    public string QuestionText { get; set; } = null!;
    public int MinScore { get; set; }
    public int MaxScore { get; set; }
    public bool IsActive { get; set; }
}
