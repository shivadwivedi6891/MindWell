using MentalHealth.Repository.Data;
using MentalHealth.Repository.Entities;
using MentalHealth.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MentalHealth.Repository.Implementations;

public class MoodEntryRepository : IMoodEntryRepository
{
    private readonly MentalHealthDbContext _context;
    public MoodEntryRepository(MentalHealthDbContext context) => _context = context;

    public async Task<MoodEntry?> GetTodayAsync(Guid userId, DateTime date)
    {
        return await _context.MoodEntries
            .FirstOrDefaultAsync(e =>
                e.UserId == userId &&
                e.EntryDate.Date == date.Date);
    }

    public async Task AddAsync(MoodEntry entry)
    {
        await _context.MoodEntries.AddAsync(entry);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
