using MentalHealth.Repository.Entities;

namespace MentalHealth.Repository.Interfaces;

public interface IGroupTopicRepository
{
    Task<GroupTopic?> GetByIdAsync(Guid id);
    Task AddAsync(GroupTopic topic);
    Task<List<GroupTopic>> GetAllAsync();
    Task SaveChangesAsync();
}
