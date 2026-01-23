using MentalHealth.Repository.Entities;
using MentalHealth.Shared.DTOs.ExpertInvitation;

namespace MentalHealth.Repository.Interfaces;

public interface IExpertInvitationRepository
{
    Task AddAsync(ExpertInvitation invitation);
    Task<List<ExpertInvitation>> GetPendingByUserAsync(Guid userId);
    Task<ExpertInvitation?> GetByIdAsync(Guid id);
    Task<List<ExpertSentInvitationDto>> GetSentByExpertAsync(Guid expertId);
    

    Task SaveChangesAsync();
}
