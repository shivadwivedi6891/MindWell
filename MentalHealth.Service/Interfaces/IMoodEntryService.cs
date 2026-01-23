using MentalHealth.Shared.DTOs.Mood;

namespace MentalHealth.Service.Interfaces;

public interface IMoodEntryService
{
    Task<MoodEntryResponseDto> SubmitAsync(Guid userId, MoodEntryCreateDto dto);
    Task<MoodEntryResponseDto?> GetTodayAsync(Guid userId);
}
