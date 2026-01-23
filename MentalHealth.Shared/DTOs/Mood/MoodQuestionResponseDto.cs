namespace MentalHealth.Shared.DTOs.Mood;

public class MoodQuestionResponseDto
{
    public Guid Id { get; set; }
    public string QuestionText { get; set; } = null!;
    public int MinScore { get; set; }
    public int MaxScore { get; set; }
}
