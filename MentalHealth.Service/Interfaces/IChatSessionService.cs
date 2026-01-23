using MentalHealth.Shared.DTOs.Chat;
using MentalHealth.Shared.DTOs.Session;

namespace MentalHealth.Service.Interfaces;

public interface IChatSessionService
{
    Task<Guid> CreateOneToOneSessionAsync(Guid userId, Guid expertId, bool isAnonymous);
    Task<Guid> CreatePeerSessionAsync(Guid userA, Guid userB);
     Task<List<ChatSessionListDto>> GetActiveSessions(Guid userId);
    Task<ChatSessionDetailDto> GetSessionDetail(Guid sessionId, Guid userId);
    Task<List<ChatMessageDto>> GetMessages(Guid sessionId, Guid userId, int skip, int take);
}
