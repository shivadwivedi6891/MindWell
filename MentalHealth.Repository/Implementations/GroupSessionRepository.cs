using MentalHealth.Repository.Data;
using MentalHealth.Repository.Entities;
using MentalHealth.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MentalHealth.Repository.Implementations;

public class GroupSessionRepository : IGroupSessionRepository
{
    private readonly MentalHealthDbContext _context;

    public GroupSessionRepository(MentalHealthDbContext context)
    {
        _context = context;
    }

    public async Task<GroupSession?> GetByIdAsync(Guid id)
    {
        return await _context.GroupSessions
            .Include(gs => gs.Topic)
            .Include(gs => gs.ChatSession)
            .FirstOrDefaultAsync(gs => gs.Id == id);
    }

    public async Task<GroupSession?> GetByTopicIdAsync(Guid topicId)
    {
        return await _context.GroupSessions
            .Include(gs => gs.Topic)
            .Include(gs => gs.ChatSession)
            .FirstOrDefaultAsync(gs => gs.TopicId == topicId && gs.ChatSession.IsActive);
    }

    public async Task AddAsync(GroupSession groupSession)
    {
        await _context.GroupSessions.AddAsync(groupSession);
    }

    public Task UpdateAsync(GroupSession groupSession)
    {
        _context.GroupSessions.Update(groupSession);
        return Task.CompletedTask;
    }

    public async Task<List<GroupSession>> GetAllActiveAsync()
    {
        return await _context.GroupSessions
            .Include(gs => gs.Topic)
            .Include(gs => gs.ChatSession)
            .Where(gs => gs.ChatSession.IsActive)
            .OrderByDescending(gs => gs.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<GroupSession>> GetAllAsync()
    {
        return await _context.GroupSessions
            .Include(gs => gs.Topic)
            .Include(gs => gs.ChatSession)
            .OrderByDescending(gs => gs.CreatedAt)
            .ToListAsync();
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}