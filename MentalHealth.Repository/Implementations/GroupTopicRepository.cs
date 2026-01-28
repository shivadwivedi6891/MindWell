using MentalHealth.Repository.Data;
using MentalHealth.Repository.Entities;
using MentalHealth.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MentalHealth.Repository.Implementations;

public class GroupTopicRepository : IGroupTopicRepository
{
    private readonly MentalHealthDbContext _context;

    public GroupTopicRepository(MentalHealthDbContext context)
    {
        _context = context;
    }

    public async Task<GroupTopic?> GetByIdAsync(Guid id)
    {
        return await _context.GroupTopics.FindAsync(id);
    }

    public async Task AddAsync(GroupTopic topic)
    {
        await _context.GroupTopics.AddAsync(topic);
    }

    public async Task<List<GroupTopic>> GetAllAsync()
    {
        return await _context.GroupTopics
            .OrderBy(t => t.Title)
            .ToListAsync();
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
