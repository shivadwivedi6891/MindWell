using MentalHealth.Repository.Data;
using MentalHealth.Repository.Entities;
using MentalHealth.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MentalHealth.Repository.Implementations;

public class ChatParticipantRepository : IChatParticipantRepository
{
    private readonly MentalHealthDbContext _context;

    public ChatParticipantRepository(MentalHealthDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(ChatParticipant participant)
    {
        await _context.ChatParticipants.AddAsync(participant);
    }

    public async Task AddRangeAsync(IEnumerable<ChatParticipant> participants)
    {
        await _context.ChatParticipants.AddRangeAsync(participants);
    }

    public async Task<List<ChatParticipant>> GetBySessionIdAsync(Guid sessionId)
    {
        return await _context.ChatParticipants
            .Where(p => p.ChatSessionId == sessionId)
            .ToListAsync();
    }

    public async Task<ChatParticipant?> GetByChatSessionAndUserAsync(Guid chatSessionId, Guid userId)
    {
        return await _context.ChatParticipants
            .FirstOrDefaultAsync(cp => cp.ChatSessionId == chatSessionId && cp.UserId == userId);
    }

    public Task UpdateAsync(ChatParticipant participant)
    {
        _context.ChatParticipants.Update(participant);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(Guid participantId)
    {
        var participant = await _context.ChatParticipants.FindAsync(participantId);
        if (participant != null)
        {
            _context.ChatParticipants.Remove(participant);
        }
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
