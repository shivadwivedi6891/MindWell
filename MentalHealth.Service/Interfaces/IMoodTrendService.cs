using MentalHealth.Shared.DTOs.Mood;

namespace MentalHealth.Service.Interfaces;

public interface IMoodTrendService
{
    Task UpdateTrendsAsync(Guid userId);
    Task<List<MoodTrendDto>> GetTrendsAsync(Guid userId);
    Task<List<MoodHistoryItemDto>> GetHistoryAsync(Guid userId);
}
