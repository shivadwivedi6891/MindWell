using MentalHealth.Repository.Data;
using MentalHealth.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MentalHealth.Repository.Implementations;

public class GroupParticipantRepository : IGroupParticipantRepository
{
    private readonly MentalHealthDbContext _context;

    public GroupParticipantRepository(MentalHealthDbContext context)
    {
        _context = context;
    }

    public async Task<int> GetParticipantCountAsync(Guid chatSessionId)
    {
        return await _context.ChatParticipants
            .Where(cp => cp.ChatSessionId == chatSessionId)
            .CountAsync();
    }

    public async Task<string> GetAnonymousNameAsync(Guid chatSessionId, Guid userId)
    {
        var participant = await _context.ChatParticipants
            .Where(cp => cp.ChatSessionId == chatSessionId && cp.UserId == userId)
            .FirstOrDefaultAsync();

        if (participant == null)
            return "User-Unknown";

        // Count how many participants joined before this user (including this user)
        var joinPosition = await _context.ChatParticipants
            .Where(cp => cp.ChatSessionId == chatSessionId && cp.JoinedAt <= participant.JoinedAt)
            .CountAsync();

        return $"User-{joinPosition}";
    }

    public async Task<bool> IsUserParticipantAsync(Guid chatSessionId, Guid userId)
    {
        return await _context.ChatParticipants
            .AnyAsync(cp => cp.ChatSessionId == chatSessionId && cp.UserId == userId);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}