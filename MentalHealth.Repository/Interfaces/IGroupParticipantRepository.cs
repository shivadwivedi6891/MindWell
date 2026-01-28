namespace MentalHealth.Repository.Interfaces;

public interface IGroupParticipantRepository
{
    Task<int> GetParticipantCountAsync(Guid chatSessionId);
    Task<string> GetAnonymousNameAsync(Guid chatSessionId, Guid userId);
    Task<bool> IsUserParticipantAsync(Guid chatSessionId, Guid userId);
    Task SaveChangesAsync();
}