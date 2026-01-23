using MentalHealth.Shared.DTOs.Mood;

namespace MentalHealth.Service.Interfaces;

public interface IMoodQuestionService
{
    Task CreateAsync(MoodQuestionCreateDto dto);
    Task<List<MoodQuestionResponseDto>> GetActiveAsync();
    Task ToggleActiveAsync(Guid id);
}
