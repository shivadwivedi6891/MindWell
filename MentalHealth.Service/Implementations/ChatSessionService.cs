using MentalHealth.Repository.Entities;
using MentalHealth.Repository.Interfaces;
using MentalHealth.Service.Interfaces;
using MentalHealth.Shared.DTOs.Chat;
using MentalHealth.Shared.DTOs.Session;

namespace MentalHealth.Service.Implementations;

public class ChatSessionService : IChatSessionService
{
    private readonly IChatSessionRepository _sessionRepo;
    private readonly IChatParticipantRepository _participantRepo;

    public ChatSessionService(
        IChatSessionRepository sessionRepo,
        IChatParticipantRepository participantRepo)
    {
        _sessionRepo = sessionRepo;
        _participantRepo = participantRepo;
    }


  public Task<List<ChatSessionListDto>> GetActiveSessions(Guid userId)
        => _sessionRepo.GetActiveSessions(userId);

    public Task<ChatSessionDetailDto> GetSessionDetail(Guid sessionId, Guid userId)
        => _sessionRepo.GetSessionDetail(sessionId, userId);

    public Task<List<ChatMessageDto>> GetMessages(Guid sessionId, Guid userId, int skip, int take)
        => _sessionRepo.GetMessages(sessionId, userId, skip, take);

   

public async Task<Guid> CreatePeerSessionAsync(Guid userA, Guid userB)
{
    // 🔒 SERIALIZED TRANSACTION
    using var tx = await _sessionRepo.BeginTransactionAsync();

    var existing = await _sessionRepo.GetPeerSessionAsync(userA, userB);
    if (existing != null)
    {
        await tx.CommitAsync();
        return existing.Id;
    }

    var session = new ChatSession
    {
        Id = Guid.NewGuid(),
        SessionType = "Peer",
        IsActive = true,
        StartedAt = DateTime.UtcNow,
        CreatedAt = DateTime.UtcNow
    };

    await _sessionRepo.AddAsync(session);

    await _participantRepo.AddAsync(new ChatParticipant
    {
        Id = Guid.NewGuid(),
        ChatSessionId = session.Id,
        UserId = userA,
        Role = "User",
        JoinedAt = DateTime.UtcNow
    });

    await _participantRepo.AddAsync(new ChatParticipant
    {
        Id = Guid.NewGuid(),
        ChatSessionId = session.Id,
        UserId = userB,
        Role = "User",
        JoinedAt = DateTime.UtcNow
    });

    await _sessionRepo.SaveChangesAsync();
    await tx.CommitAsync();

    return session.Id;
}

    public async Task<Guid> CreateOneToOneSessionAsync(
        Guid userId,
        Guid expertId,
        bool isAnonymous)
    {
        // 1️⃣ Prevent duplicate session
        var existing = await _sessionRepo.GetByParticipantsAsync(userId, expertId);
        if (existing != null && existing.IsActive)
            return existing.Id;

        // 2️⃣ Create session
        var session = new ChatSession
        {
            Id = Guid.NewGuid(),
            SessionType = "OneToOne",
            IsAnonymous = isAnonymous,
            IsActive = true,
            IsPaused = false,
            StartedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        await _sessionRepo.AddAsync(session);

        // 3️⃣ Add user participant
        await _participantRepo.AddAsync(new ChatParticipant
        {
            Id = Guid.NewGuid(),
            ChatSessionId = session.Id,
            UserId = userId,
            Role = "User",
            IsIdentityRevealed = !isAnonymous,
            JoinedAt = DateTime.UtcNow
        });

        // 4️⃣ Add expert participant
        await _participantRepo.AddAsync(new ChatParticipant
        {
            Id = Guid.NewGuid(),
            ChatSessionId = session.Id,
            UserId = expertId,
            Role = "Expert",
            IsIdentityRevealed = false,
            JoinedAt = DateTime.UtcNow
        });

        await _sessionRepo.SaveChangesAsync();

        return session.Id;
    }
}
