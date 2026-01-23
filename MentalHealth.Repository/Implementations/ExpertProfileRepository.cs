using MentalHealth.Repository.Data;
using MentalHealth.Repository.Entities;
using MentalHealth.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MentalHealth.Repository.Implementations;

public class ExpertProfileRepository : IExpertProfileRepository
{
    private readonly MentalHealthDbContext _context;

    public ExpertProfileRepository(MentalHealthDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(ExpertProfile profile)
    {
        await _context.ExpertProfiles.AddAsync(profile);
    }

    public async Task<ExpertProfile?> GetByUserIdAsync(Guid userId)
    {
        return await _context.ExpertProfiles
            .Include(e => e.User)
            .FirstOrDefaultAsync(e => e.UserId == userId);
    }

    public async Task<ExpertProfile?> GetByIdAsync(Guid id)
    {
        return await _context.ExpertProfiles
            .Include(e => e.User)
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<List<ExpertProfile>> GetPendingAsync()
    {
        return await _context.ExpertProfiles
            .Include(e => e.User)
            .Where(e => !e.IsApproved)
            .ToListAsync();
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }

    public Task DeleteAsync(ExpertProfile profile)
    {
        _context.ExpertProfiles.Remove(profile);
        return Task.CompletedTask;
    }
}
