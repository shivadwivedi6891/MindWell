namespace MentalHealth.Repository.Entities;

public class MoodEntry : BaseEntity
{
    public Guid UserId { get; set; }
    public DateTime EntryDate { get; set; }
    public int TotalScore { get; set; }

    public User User { get; set; } = null!;
    public ICollection<MoodAnswer> Answers { get; set; } = new List<MoodAnswer>();
}
