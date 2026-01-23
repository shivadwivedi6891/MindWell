using MentalHealth.Repository.Entities;

namespace MentalHealth.Repository.Interfaces;

public interface IChatParticipantRepository
{
     Task AddAsync(ChatParticipant participant);
    Task AddRangeAsync(IEnumerable<ChatParticipant> participants);
    Task<List<ChatParticipant>> GetBySessionIdAsync(Guid sessionId);
    Task SaveChangesAsync();
}