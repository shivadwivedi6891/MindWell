using MentalHealth.Repository.Entities;
using MentalHealth.Shared.DTOs.Chat;
using MentalHealth.Shared.DTOs.Session;
using Microsoft.EntityFrameworkCore.Storage;

namespace MentalHealth.Repository.Interfaces;

public interface IChatSessionRepository
{
    Task AddAsync(ChatSession session);
    Task<ChatSession?> GetByParticipantsAsync(Guid userId, Guid expertId);

     Task<List<ChatSessionListDto>> GetActiveSessions(Guid userId);
     Task<ChatSessionDetailDto> GetSessionDetail(Guid sessionId, Guid userId);
      Task<List<ChatMessageDto>> GetMessages(Guid sessionId, Guid userId, int skip, int take);
   
    Task<ChatSession?> GetPeerSessionAsync(Guid userA, Guid userB);
    Task<ChatSession?> GetByIdAsync(Guid id);
    Task<ChatSession?> GetByIdWithParticipantsAsync(Guid sessionId);

    Task<IDbContextTransaction> BeginTransactionAsync();

    Task SaveChangesAsync();
}
