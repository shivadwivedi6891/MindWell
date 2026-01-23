using MentalHealth.Repository.Entities;
using MentalHealth.Repository.Interfaces;
using MentalHealth.Service.Interfaces;
using MentalHealth.Shared.DTOs.Chat;

namespace MentalHealth.Service.Implementations;

public class PeerChatService : IPeerChatService
{
    private readonly IChatSessionRepository _sessionRepo;
    private readonly IChatParticipantRepository _participantRepo;
    private readonly IMessageRepository _messageRepo;
    private readonly IUserRepository _userRepo;

    public PeerChatService(
        IChatSessionRepository sessionRepo,
        IChatParticipantRepository participantRepo,
        IMessageRepository messageRepo,
        IUserRepository userRepo)
    {
        _sessionRepo = sessionRepo;
        _participantRepo = participantRepo;
        _messageRepo = messageRepo;
        _userRepo = userRepo;
    }

    public async Task<(Guid sessionId, bool isNew)> CreateOrGetPeerSessionAsync(Guid userA, Guid userB)
    {
        // 1️⃣ Validate users are different
        if (userA == userB)
            throw new ArgumentException("Cannot create a peer session with the same user.");

        // 2️⃣ Verify both users exist
        var userAExists = await _userRepo.GetByIdAsync(userA);
        var userBExists = await _userRepo.GetByIdAsync(userB);
        
        if (userAExists == null || userBExists == null)
            throw new ArgumentException("One or both users do not exist.");

        // 3️⃣ Look up existing peer session (order-independent)
        var existing = await _sessionRepo.GetPeerSessionAsync(userA, userB);
        if (existing != null)
            return (existing.Id, false);

        // 4️⃣ Create new session
        var session = new ChatSession
        {
            Id = Guid.NewGuid(),
            SessionType = "Peer",
            IsAnonymous = false,
            IsActive = true,
            IsPaused = false,
            StartedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        await _sessionRepo.AddAsync(session);

        // 5️⃣ Add both users as participants
        var participantA = new ChatParticipant
        {
            Id = Guid.NewGuid(),
            ChatSessionId = session.Id,
            UserId = userA,
            Role = "User",
            IsIdentityRevealed = false,
            JoinedAt = DateTime.UtcNow
        };

        var participantB = new ChatParticipant
        {
            Id = Guid.NewGuid(),
            ChatSessionId = session.Id,
            UserId = userB,
            Role = "User",
            IsIdentityRevealed = false,
            JoinedAt = DateTime.UtcNow
        };

        await _participantRepo.AddRangeAsync(new[] { participantA, participantB });

        // 6️⃣ Persist changes
        await _sessionRepo.SaveChangesAsync();

        return (session.Id, true);
    }

    public async Task<PeerChatSessionDto?> GetPeerSessionAsync(Guid sessionId, Guid currentUserId)
    {
        var session = await _sessionRepo.GetByIdAsync(sessionId);

        if (session == null || session.SessionType != "Peer")
            return null;

        // Validate current user is a participant
        if (!session.Participants.Any(p => p.UserId == currentUserId))
            return null;

        // Ensure exactly 2 participants
        if (session.Participants.Count != 2)
            return null;

        var otherParticipant = session.Participants.FirstOrDefault(p => p.UserId != currentUserId);

        return new PeerChatSessionDto
        {
            SessionId = session.Id,
            SessionType = session.SessionType,
            OtherUserId = otherParticipant!.UserId,
            OtherUserIsAnonymous = !otherParticipant.IsIdentityRevealed,
            CreatedAt = session.CreatedAt,
            IsActive = session.IsActive,
            ParticipantCount = session.Participants.Count
        };
    }

    public async Task<List<ChatMessageDto>> GetMessageHistoryAsync(Guid sessionId, Guid currentUserId, int page = 1, int pageSize = 50)
    {
        // 1️⃣ Validate session exists and user is participant
        var session = await _sessionRepo.GetByIdAsync(sessionId);

        if (session == null || session.SessionType != "Peer")
            throw new ArgumentException("Peer chat session not found.");

        if (!session.Participants.Any(p => p.UserId == currentUserId))
            throw new ArgumentException("Not a participant in this session.");

        // 2️⃣ Get paginated messages (newest first)
        var messages = await _messageRepo.GetBySessionIdAsync(sessionId, page, pageSize);

        // 3️⃣ Map to DTOs with anonymity masking
        var messageDtos = messages
            .OrderBy(m => m.SentAt) // Return in chronological order for display
            .Select(m =>
            {
                var sender = session.Participants.FirstOrDefault(p => p.UserId == m.SenderId);
                var isAnonymous = sender == null || !sender.IsIdentityRevealed;

                return new ChatMessageDto
                {
                    ChatSessionId = m.ChatSessionId,
                    SenderId = m.SenderId,
                    Content = m.Content,
                    SentAt = m.SentAt,
                    IsAnonymous = isAnonymous
                };
            })
            .ToList();

        return messageDtos;
    }
}
