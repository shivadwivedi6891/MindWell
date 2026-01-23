using MentalHealth.Repository.Entities;

namespace MentalHealth.Repository.Interfaces;

public interface IExpertInvitationRepository
{
    Task AddAsync(ExpertInvitation invitation);
    Task<List<ExpertInvitation>> GetPendingByUserAsync(Guid userId);
    Task<ExpertInvitation?> GetByIdAsync(Guid id);
    Task SaveChangesAsync();
}
