using MentalHealth.Repository.Data;
using MentalHealth.Repository.Entities;
using MentalHealth.Repository.Interfaces;

namespace MentalHealth.Repository.Implementations;

public class MoodAnswerRepository : IMoodAnswerRepository
{
    private readonly MentalHealthDbContext _context;
    public MoodAnswerRepository(MentalHealthDbContext context) => _context = context;

    public async Task AddRangeAsync(IEnumerable<MoodAnswer> answers)
    {
        await _context.MoodAnswers.AddRangeAsync(answers);
    }
}
