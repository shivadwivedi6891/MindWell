using MentalHealth.Repository.Entities;
using MentalHealth.Repository.Interfaces;
using MentalHealth.Service.Interfaces;
using MentalHealth.Shared.DTOs.Mood;

namespace MentalHealth.Service.Implementations;

public class MoodQuestionService : IMoodQuestionService
{
    private readonly IMoodQuestionRepository _repository;

    public MoodQuestionService(IMoodQuestionRepository repository)
    {
        _repository = repository;
    }

    public async Task CreateAsync(MoodQuestionCreateDto dto)
    {
        var question = new MoodQuestion
        {
            Id = Guid.NewGuid(),
            QuestionText = dto.QuestionText,
            MinScore = dto.MinScore,
            MaxScore = dto.MaxScore,
            IsActive = true
        };

        await _repository.AddAsync(question);
        await _repository.SaveChangesAsync();
    }

    public async Task<List<MoodQuestionResponseDto>> GetActiveAsync()
    {
        var questions = await _repository.GetActiveAsync();

        return questions.Select(q => new MoodQuestionResponseDto
        {
            Id = q.Id,
            QuestionText = q.QuestionText,
            MinScore = q.MinScore,
            MaxScore = q.MaxScore
        }).ToList();
    }

    public async Task ToggleActiveAsync(Guid id)
    {
        var question = await _repository.GetByIdAsync(id);
        if (question == null)
            throw new Exception("Mood question not found");

        question.IsActive = !question.IsActive;
        await _repository.SaveChangesAsync();
    }
}
