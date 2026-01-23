using MentalHealth.Repository.Entities;


namespace MentalHealth.Repository.Interfaces;

public interface IMoodTrendRepository
{
    
    Task<MoodTrend?>GetAsync(Guid userId, string period,DateTime periodStart);
    Task AddAsync(MoodTrend trend);
    Task UpdateAsync(MoodTrend trend);

    Task<List<MoodTrend>> GetByUserAsync(Guid userId);

    Task<List<MoodTrend>> GetLowMoodUsersAsync();


    Task SaveChangesAsync();
}