using MentalHealth.Repository.Data;
using MentalHealth.Repository.Entities;
using MentalHealth.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;


namespace MentalHealth.Repository.Implementations;


public class UserChatRequestRepository : IUserChatRequestRepository
{
    private readonly MentalHealthDbContext _context;

    public UserChatRequestRepository(MentalHealthDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(UserChatRequest request)
    {
        await _context.UserChatRequests.AddAsync(request);
        await _context.SaveChangesAsync();
    }

    public async Task<UserChatRequest> GetByIdAsync(Guid id)
    {
        return await _context.UserChatRequests
            .Include(r => r.FromUser)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<List<UserChatRequest>> GetIncomingAsync(Guid userId)
    {
        return await _context.UserChatRequests
            .Include(r => r.FromUser)
            .Where(r => r.ToUserId == userId && r.Status == "Pending")
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<UserChatRequest> GetPendingBetweenAsync(Guid fromUserId, Guid toUserId)
    {
        return await _context.UserChatRequests.FirstOrDefaultAsync(r =>
            r.FromUserId == fromUserId &&
            r.ToUserId == toUserId &&
            r.Status == "Pending");
    }

    public async Task UpdateAsync(UserChatRequest request)
    {
        _context.UserChatRequests.Update(request);
        await _context.SaveChangesAsync();
    }
}
