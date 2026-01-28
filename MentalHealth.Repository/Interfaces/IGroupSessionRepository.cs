using MentalHealth.Repository.Entities;

namespace MentalHealth.Repository.Interfaces;

public interface IGroupSessionRepository
{
    Task<GroupSession?> GetByIdAsync(Guid id);
    Task<GroupSession?> GetByTopicIdAsync(Guid topicId);
    Task AddAsync(GroupSession groupSession);
    Task UpdateAsync(GroupSession groupSession);
    Task<List<GroupSession>> GetAllActiveAsync();
    Task<List<GroupSession>> GetAllAsync();
    Task SaveChangesAsync();
}
