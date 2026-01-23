using MentalHealth.Repository.Data;
using MentalHealth.Repository.Entities;
using MentalHealth.Repository.Interfaces;
using MentalHealth.Shared.DTOs.ExpertInvitation;
using Microsoft.EntityFrameworkCore;

namespace MentalHealth.Repository.Implementations;

public class ExpertInvitationRepository : IExpertInvitationRepository
{
    private readonly MentalHealthDbContext _context;
    public ExpertInvitationRepository(MentalHealthDbContext context) => _context = context;

    public async Task AddAsync(ExpertInvitation invitation)
    {
        await _context.ExpertInvitations.AddAsync(invitation);
    }

    public async Task<List<ExpertInvitation>> GetPendingByUserAsync(Guid userId)
    {
        return await _context.ExpertInvitations
            .Include(i => i.Expert)
            .Where(i => i.UserId == userId && i.Status == "Pending")
            .ToListAsync();
    }

    public async Task<ExpertInvitation?> GetByIdAsync(Guid id)
    {
        return await _context.ExpertInvitations.FindAsync(id);
    }

    public async Task<List<ExpertSentInvitationDto>> GetSentByExpertAsync(Guid expertId)
{
    return await _context.ExpertInvitations
        .Where(i => i.ExpertId == expertId)
        .Include(i => i.User)
        .OrderByDescending(i => i.CreatedAt)
        .Select(i => new ExpertSentInvitationDto
        {
            Id = i.Id,
            UserId = i.UserId,
            UserDisplayName = i.User.DisplayName,

            Message = i.Message,
            Status = i.Status,
            CreatedAt = i.CreatedAt
        })
        .ToListAsync();
}


    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
