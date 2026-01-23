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

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
