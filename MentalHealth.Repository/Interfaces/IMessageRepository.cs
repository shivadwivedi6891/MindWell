using MentalHealth.Repository.Entities;

namespace MentalHealth.Repository.Interfaces;

public interface IMessageRepository
{
    Task AddAsync(Message message);
    Task<List<Message>> GetBySessionAsync(Guid chatSessionId);
    Task<List<Message>> GetBySessionIdAsync(Guid sessionId, int page, int pageSize);
    Task SaveChangesAsync();
}
