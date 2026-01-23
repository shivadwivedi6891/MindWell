using MentalHealth.Repository.Data;
using MentalHealth.Repository.Entities;
using MentalHealth.Repository.Interfaces;
using MentalHealth.Service.Interfaces;
using MentalHealth.Shared.DTOs.Mood;
using Microsoft.EntityFrameworkCore;

namespace MentalHealth.Service.Implementations;

public class MoodTrendService : IMoodTrendService
{
    private const int LOW_THRESHOLD = 100;

    private readonly MentalHealthDbContext _context;
    private readonly IMoodTrendRepository _trendRepo;

    public MoodTrendService(
        MentalHealthDbContext context,
        IMoodTrendRepository trendRepo)
    {
        _context = context;
        _trendRepo = trendRepo;
    }

    public async Task UpdateTrendsAsync(Guid userId)
    {
        var entries = await _context.MoodEntries
            .Where(e => e.UserId == userId)
            .ToListAsync();

        await UpsertTrend(userId, entries, "Daily", e => e.EntryDate.Date);
        await UpsertTrend(userId, entries, "Weekly", e => StartOfWeek(e.EntryDate));
        await UpsertTrend(userId, entries, "Monthly", e => new DateTime(e.EntryDate.Year, e.EntryDate.Month, 1));
    }

    private async Task UpsertTrend(
        Guid userId,
        List<MoodEntry> entries,
        string period,
        Func<MoodEntry, DateTime> keySelector)
    {
        var groups = entries.GroupBy(keySelector);

        foreach (var g in groups)
        {
            var avg = g.Average(e => e.TotalScore);
            var isLow = avg < LOW_THRESHOLD;

            var existing = await _trendRepo.GetAsync(userId, period, g.Key);

            if (existing == null)
            {
                await _trendRepo.AddAsync(new MoodTrend
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    Period = period,
                    PeriodStart = g.Key,
                    AverageScore = avg,
                    IsLow = isLow
                });
            }
            else
            {
                existing.AverageScore = avg;
                existing.IsLow = isLow;
                await _trendRepo.UpdateAsync(existing);
            }
        }

        await _trendRepo.SaveChangesAsync();
    }

    private static DateTime StartOfWeek(DateTime date)
    {
        int diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
        return date.AddDays(-diff).Date;
    }

    public async Task<List<MoodTrendDto>> GetTrendsAsync(Guid userId)
    {
        var trends = await _trendRepo.GetByUserAsync(userId);

        return trends.Select(t => new MoodTrendDto
        {
            Period = t.Period,
            PeriodStart = t.PeriodStart,
            AverageScore = t.AverageScore,
            IsLow = t.IsLow
        }).ToList();
    }

    public async Task<List<MoodHistoryItemDto>> GetHistoryAsync(Guid userId)
    {
        return await _context.MoodEntries
            .Where(e => e.UserId == userId)
            .OrderByDescending(e => e.EntryDate)
            .Select(e => new MoodHistoryItemDto
            {
                EntryDate = e.EntryDate,
                TotalScore = e.TotalScore
            })
            .ToListAsync();
    }
}
