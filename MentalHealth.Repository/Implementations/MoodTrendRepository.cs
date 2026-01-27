using MentalHealth.Repository.Data;
using MentalHealth.Repository.Entities;
using MentalHealth.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;


namespace MentalHealth.Repository.Implementations;

public class MoodTrendRepository : IMoodTrendRepository
{
    private readonly MentalHealthDbContext _context;
    public MoodTrendRepository(MentalHealthDbContext context) => _context = context;

    public async Task<MoodTrend?> GetAsync(Guid userId, string period, DateTime periodStart)
    {
        return await _context.MoodTrends.FirstOrDefaultAsync(t =>
            t.UserId == userId &&
            t.Period == period &&
            t.PeriodStart == periodStart);
    }

    public async Task AddAsync(MoodTrend trend)
    {
        await _context.MoodTrends.AddAsync(trend);
    }

    public Task UpdateAsync(MoodTrend trend)
    {
        _context.MoodTrends.Update(trend);
        return Task.CompletedTask;
    }

    public async Task<List<MoodTrend>> GetByUserAsync(Guid userId)
    {
        return await _context.MoodTrends
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.PeriodStart)
            .ToListAsync();
    }


public async Task<List<MoodTrend>> GetLowMoodUsersAsync(Guid currentUserId)
{
    return await _context.MoodTrends
        .Where(t => t.IsLow
            && !_context.UserChatRequests.Any(cr =>
                cr.FromUserId == currentUserId
                && cr.ToUserId == t.UserId
                && cr.Status == "Pending"))
        .OrderBy(t => t.AverageScore)
        .ToListAsync();
}

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
