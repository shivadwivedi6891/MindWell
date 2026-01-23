namespace MentalHealth.Shared.DTOs.Mood;

public class MoodEntryResponseDto
{
    public Guid EntryId { get; set; }
    public DateTime EntryDate { get; set; }
    public int TotalScore { get; set; }
}
