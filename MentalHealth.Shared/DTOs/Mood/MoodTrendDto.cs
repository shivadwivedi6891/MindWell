namespace MentalHealth.Shared.DTOs.Mood;

public class MoodTrendDto
{
    public string Period { get; set; } = null!; // Daily / Weekly / Monthly
    public DateTime PeriodStart { get; set; }
    public double AverageScore { get; set; }
    public bool IsLow { get; set; }
}
