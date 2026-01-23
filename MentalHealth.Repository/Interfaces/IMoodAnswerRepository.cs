using MentalHealth.Repository.Entities;

namespace MentalHealth.Repository.Interfaces;

public interface IMoodAnswerRepository
{
    Task AddRangeAsync(IEnumerable<MoodAnswer> answers);
}
