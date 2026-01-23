namespace MentalHealth.Shared.DTOs.Mood;

public class MoodEntryCreateDto
{
    public List<MoodAnswerDto> Answers { get; set; } = new();
}
