using MentalHealth.Repository.Data;
using MentalHealth.Repository.Entities;
using MentalHealth.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MentalHealth.Repository.Implementations;

public class MoodQuestionRepository : IMoodQuestionRepository
{
    private readonly MentalHealthDbContext _context;

    public MoodQuestionRepository(MentalHealthDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(MoodQuestion question)
    {
        await _context.MoodQuestions.AddAsync(question);
    }

    public async Task<List<MoodQuestion>> GetActiveAsync()
    {
        return await _context.MoodQuestions
            .Where(q => q.IsActive)
            .OrderBy(q => q.CreatedAt)
            .ToListAsync();
    }

    public async Task<MoodQuestion?> GetByIdAsync(Guid id)
    {
        return await _context.MoodQuestions.FindAsync(id);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
