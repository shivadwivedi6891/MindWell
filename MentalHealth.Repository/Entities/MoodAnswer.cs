namespace MentalHealth.Repository.Entities;

public class MoodAnswer : BaseEntity
{
    public Guid MoodEntryId { get; set; }
    public Guid QuestionId { get; set; }
    public int Score { get; set; }

    public MoodEntry MoodEntry { get; set; } = null!;
    public MoodQuestion Question { get; set; } = null!;
}
