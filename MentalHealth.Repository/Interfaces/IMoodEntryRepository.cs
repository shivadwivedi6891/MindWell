using MentalHealth.Repository.Entities;

namespace MentalHealth.Repository.Interfaces;

public interface IMoodEntryRepository
{
    Task<MoodEntry?> GetTodayAsync(Guid userId, DateTime date);
    Task AddAsync(MoodEntry entry);
    Task SaveChangesAsync();
}
