using MentalHealth.Repository.Data;
using MentalHealth.Repository.Entities;
using MentalHealth.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MentalHealth.Repository.Implementations;

public class MessageRepository : IMessageRepository
{
    private readonly MentalHealthDbContext _context;

    public MessageRepository(MentalHealthDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Message message)
    {
        await _context.Messages.AddAsync(message);
    }

    public async Task<List<Message>> GetBySessionAsync(Guid chatSessionId)
    {
        return await _context.Messages
            .Where(m => m.ChatSessionId == chatSessionId)
            .OrderBy(m => m.SentAt)
            .ToListAsync();
    }

    public async Task<List<Message>> GetBySessionIdAsync(Guid sessionId, int page, int pageSize)
    {
        var skip = (page - 1) * pageSize;

        return await _context.Messages
            .Where(m => m.ChatSessionId == sessionId && !m.IsDeleted)
            .OrderByDescending(m => m.SentAt)
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
