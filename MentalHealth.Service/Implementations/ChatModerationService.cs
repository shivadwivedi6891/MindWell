using MentalHealth.Repository.Entities;
using MentalHealth.Repository.Interfaces;
using MentalHealth.Service.Interfaces;
using MentalHealth.Shared.DTOs.Chat;

namespace MentalHealth.Service.Implementations;

/// <summary>
/// Service for managing chat moderation and safety controls
/// </summary>
public class ChatModerationService : IChatModerationService
{
    private readonly IChatSessionRepository _sessionRepo;
    private readonly IMessageRepository _messageRepo;

    public ChatModerationService(
        IChatSessionRepository sessionRepo,
        IMessageRepository messageRepo)
    {
        _sessionRepo = sessionRepo;
        _messageRepo = messageRepo;
    }

    public async Task<ChatControlResponseDto> PauseSessionAsync(Guid sessionId, Guid userId)
    {
        // 1️⃣ Load session with participants
        var session = await _sessionRepo.GetByIdWithParticipantsAsync(sessionId);
        if (session == null)
            throw new ArgumentException("Chat session not found.");

        // 2️⃣ Validate user is a participant
        var participant = session.Participants.FirstOrDefault(p => p.UserId == userId);
        if (participant == null)
            throw new ArgumentException("User is not a participant in this session.");

        // 3️⃣ Validate permissions based on session type and role
        if (session.SessionType == "OneToOne" && participant.Role != "Expert")
            throw new ArgumentException("Only experts can pause one-to-one chats.");

        // For Peer chats, any participant can pause

        // 4️⃣ Check if already paused
        if (session.IsPaused)
            throw new ArgumentException("Chat session is already paused.");

        // 5️⃣ Update session state
        session.IsPaused = true;

        // 6️⃣ Create system message
        var systemMessage = new Message
        {
            Id = Guid.NewGuid(),
            ChatSessionId = sessionId,
            SenderId = userId,
            Content = $"Chat paused by {participant.Role.ToLower()}",
            IsSystemMessage = true,
            SentAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        await _messageRepo.AddAsync(systemMessage);

        // 7️⃣ Save changes
        await _sessionRepo.SaveChangesAsync();

        // 8️⃣ Return response
        return new ChatControlResponseDto
        {
            SessionId = sessionId,
            Action = "paused",
            IsPaused = session.IsPaused,
            IsActive = session.IsActive,
            Timestamp = DateTime.UtcNow,
            ActionBy = userId
        };
    }

    public async Task<ChatControlResponseDto> ResumeSessionAsync(Guid sessionId, Guid userId)
    {
        // 1️⃣ Load session with participants
        var session = await _sessionRepo.GetByIdWithParticipantsAsync(sessionId);
        if (session == null)
            throw new ArgumentException("Chat session not found.");

        // 2️⃣ Validate user is a participant
        var participant = session.Participants.FirstOrDefault(p => p.UserId == userId);
        if (participant == null)
            throw new ArgumentException("User is not a participant in this session.");

        // 3️⃣ Validate permissions based on session type and role
        if (session.SessionType == "OneToOne" && participant.Role != "Expert")
            throw new ArgumentException("Only experts can resume one-to-one chats.");

        // For Peer chats, any participant can resume

        // 4️⃣ Check if not paused
        if (!session.IsPaused)
            throw new ArgumentException("Chat session is not paused.");

        // 5️⃣ Update session state
        session.IsPaused = false;

        // 6️⃣ Create system message
        var systemMessage = new Message
        {
            Id = Guid.NewGuid(),
            ChatSessionId = sessionId,
            SenderId = userId,
            Content = $"Chat resumed by {participant.Role.ToLower()}",
            IsSystemMessage = true,
            SentAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        await _messageRepo.AddAsync(systemMessage);

        // 7️⃣ Save changes
        await _sessionRepo.SaveChangesAsync();

        // 8️⃣ Return response
        return new ChatControlResponseDto
        {
            SessionId = sessionId,
            Action = "resumed",
            IsPaused = session.IsPaused,
            IsActive = session.IsActive,
            Timestamp = DateTime.UtcNow,
            ActionBy = userId
        };
    }

    public async Task<ChatControlResponseDto> EndSessionAsync(Guid sessionId, Guid userId)
    {
        // 1️⃣ Load session with participants
        var session = await _sessionRepo.GetByIdWithParticipantsAsync(sessionId);
        if (session == null)
            throw new ArgumentException("Chat session not found.");

        // 2️⃣ Validate user is a participant
        var participant = session.Participants.FirstOrDefault(p => p.UserId == userId);
        if (participant == null)
            throw new ArgumentException("User is not a participant in this session.");

        // 3️⃣ Any participant can end the session (no role-based restrictions)

        // 4️⃣ Check if already ended
        if (!session.IsActive)
            throw new ArgumentException("Chat session is already ended.");

        // 5️⃣ Update session state
        session.IsActive = false;
        session.EndedAt = DateTime.UtcNow;

        // 6️⃣ Create system message
        var systemMessage = new Message
        {
            Id = Guid.NewGuid(),
            ChatSessionId = sessionId,
            SenderId = userId,
            Content = $"Chat ended by {participant.Role.ToLower()}",
            IsSystemMessage = true,
            SentAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        await _messageRepo.AddAsync(systemMessage);

        // 7️⃣ Save changes
        await _sessionRepo.SaveChangesAsync();

        // 8️⃣ Return response
        return new ChatControlResponseDto
        {
            SessionId = sessionId,
            Action = "ended",
            IsPaused = session.IsPaused,
            IsActive = session.IsActive,
            Timestamp = DateTime.UtcNow,
            ActionBy = userId
        };
    }

    public async Task<List<ChatMessageDto>> GetSessionMessagesAsync(Guid sessionId, Guid userId, int page = 1, int pageSize = 50)
    {
        // 1️⃣ Validate session exists and user is participant
        var session = await _sessionRepo.GetByIdWithParticipantsAsync(sessionId);
        if (session == null)
            throw new ArgumentException("Chat session not found.");

        if (!session.Participants.Any(p => p.UserId == userId))
            throw new ArgumentException("User is not a participant in this session.");

        // 2️⃣ Get paginated messages (newest first in query, but we'll return chronologically)
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
