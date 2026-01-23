using MentalHealth.Repository.Entities;
using MentalHealth.Repository.Interfaces;
using MentalHealth.Service.Interfaces;
using MentalHealth.Shared.DTOs.Mood;

namespace MentalHealth.Service.Implementations;

public class MoodEntryService : IMoodEntryService
{
    private readonly IMoodEntryRepository _entryRepo;
    private readonly IMoodAnswerRepository _answerRepo;
    private readonly IMoodQuestionRepository _questionRepo;

    private readonly IMoodTrendService _trendService;

    

    public MoodEntryService(
        IMoodEntryRepository entryRepo,
        IMoodAnswerRepository answerRepo,
        IMoodQuestionRepository questionRepo,
        IMoodTrendService trendService)
    {
        _entryRepo = entryRepo;
        _answerRepo = answerRepo;
        _questionRepo = questionRepo;
        _trendService = trendService;
    }

    public async Task<MoodEntryResponseDto> SubmitAsync(Guid userId, MoodEntryCreateDto dto)
    {
        var today = DateTime.UtcNow.Date;

        // 1) Only one entry per day
        var existing = await _entryRepo.GetTodayAsync(userId, today);
        if (existing != null)
            throw new Exception("Mood entry already submitted for today.");

        // 2) Load active questions
        var activeQuestions = await _questionRepo.GetActiveAsync();
        var activeMap = activeQuestions.ToDictionary(q => q.Id);

        // 3) Validate answers + compute score
        int total = 0;
        var answers = new List<MoodAnswer>();

        foreach (var a in dto.Answers)
        {
            if (!activeMap.TryGetValue(a.QuestionId, out var q))
                throw new Exception("Invalid or inactive question.");

            if (a.Score < q.MinScore || a.Score > q.MaxScore)
                throw new Exception("Score out of allowed range.");

            total += a.Score;

            answers.Add(new MoodAnswer
            {
                Id = Guid.NewGuid(),
                QuestionId = q.Id,
                Score = a.Score
            });
        }

        // 4) Save entry
        var entry = new MoodEntry
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            EntryDate = today,
            TotalScore = total
        };

        await _entryRepo.AddAsync(entry);
        await _entryRepo.SaveChangesAsync();

        // 5) Save answers (link to entry)
        answers.ForEach(a => a.MoodEntryId = entry.Id);
        await _answerRepo.AddRangeAsync(answers);
        await _entryRepo.SaveChangesAsync();
        await _trendService.UpdateTrendsAsync(userId);




        return new MoodEntryResponseDto
        {
            EntryId = entry.Id,
            EntryDate = entry.EntryDate,
            TotalScore = entry.TotalScore
        };
    }

    public async Task<MoodEntryResponseDto?> GetTodayAsync(Guid userId)
    {
        var today = DateTime.UtcNow.Date;
        var entry = await _entryRepo.GetTodayAsync(userId, today);
        if (entry == null) return null;

        return new MoodEntryResponseDto
        {
            EntryId = entry.Id,
            EntryDate = entry.EntryDate,
            TotalScore = entry.TotalScore
        };
    }
}
