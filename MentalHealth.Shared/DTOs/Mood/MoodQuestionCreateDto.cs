namespace MentalHealth.Shared.DTOs.Mood;

public class MoodQuestionCreateDto
{
    public string QuestionText { get; set; } = null!;
    public int MinScore { get; set; }
    public int MaxScore { get; set; }
}
