using MentalHealth.Repository.Entities;

namespace MentalHealth.Repository.Interfaces;

public interface IMoodQuestionRepository
{
    Task AddAsync(MoodQuestion question);
    Task<List<MoodQuestion>> GetActiveAsync();
    Task<MoodQuestion?> GetByIdAsync(Guid id);
    Task SaveChangesAsync();
}
