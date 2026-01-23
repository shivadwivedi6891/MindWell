namespace MentalHealth.Repository.Entities;

public class MoodTrend : BaseEntity
{
    public Guid UserId { get; set; }

    public string Period { get; set; } = null!; // Daily / Weekly / Monthly

    public DateTime PeriodStart { get; set; }   
    public double AverageScore { get; set; }
    public bool IsLow { get; set; }

    public User User { get; set; } = null!;
}
