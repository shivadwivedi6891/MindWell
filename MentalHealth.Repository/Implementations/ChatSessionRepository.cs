using MentalHealth.Repository.Data;
using MentalHealth.Repository.Entities;
using MentalHealth.Repository.Interfaces;
using MentalHealth.Shared.DTOs.Chat;
using MentalHealth.Shared.DTOs.Session;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace MentalHealth.Repository.Implementations;

public class ChatSessionRepository : IChatSessionRepository
{
    private readonly MentalHealthDbContext _context;

    public ChatSessionRepository(MentalHealthDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(ChatSession session)
    {
        await _context.ChatSessions.AddAsync(session);
    }

    public async Task<ChatSession?> GetByParticipantsAsync(Guid userId, Guid expertId)
    {
        return await _context.ChatSessions
            .Include(s => s.Participants)
            .Where(s => s.SessionType == "OneToOne")
            .FirstOrDefaultAsync(s =>
                s.Participants.Any(p => p.UserId == userId) &&
                s.Participants.Any(p => p.UserId == expertId));
    }



     public async Task<List<ChatSessionListDto>> GetActiveSessions(Guid userId)
    {
        var sessions = await _context.ChatSessions
            .Where(s => s.IsActive && s.Participants.Any(p => p.UserId == userId))
            .Include(s => s.Participants)
                .ThenInclude(p => p.User)
            .Include(s => s.Messages)
            .Select(s => new ChatSessionListDto
            {
                SessionId = s.Id,
                SessionType = s.SessionType,

                OtherUserId = s.Participants
                    .Where(p => p.UserId != userId)
                    .Select(p => p.UserId)
                    .FirstOrDefault(),

                OtherUserDisplayName = s.Participants
                    .Where(p => p.UserId != userId)
                    .Select(p => p.User.DisplayName)
                    .FirstOrDefault(),

                OtherUserIsAnonymous = s.Participants
                    .Where(p => p.UserId != userId)
                    .Select(p => p.User.IsAnonymous)
                    .FirstOrDefault(),

                CreatedAt = s.StartedAt,
                IsActive = s.IsActive,
                IsPaused = s.IsPaused,
                IsEnded = s.EndedAt,

                LastMessage = s.Messages
                    .OrderByDescending(m => m.SentAt)
                    .Select(m => m.Content)
                    .FirstOrDefault(),

                LastMessageAt = s.Messages
                    .OrderByDescending(m => m.SentAt)
                    .Select(m => (DateTime?)m.SentAt)
                    .FirstOrDefault(),


            })
            .ToListAsync();

        return sessions;
    }



    //----------------//

      public async Task<ChatSessionDetailDto> GetSessionDetail(Guid sessionId, Guid userId)
    {
        var session = await _context.ChatSessions
            .Include(s => s.Participants)
                .ThenInclude(p => p.User)
            .FirstOrDefaultAsync(s => s.Id == sessionId);

        if (session == null)
            throw new Exception("Session not found");

        if (!session.Participants.Any(p => p.UserId == userId))
            throw new UnauthorizedAccessException();

        return new ChatSessionDetailDto
        {
            SessionId = session.Id,
            SessionType = session.SessionType,
            IsActive = session.IsActive,
            IsPaused = session.IsPaused,
            IsEnded = session.StartedAt,
            CreatedAt = session.EndedAt,

            Participants = session.Participants.Select(p => new ChatParticipantDto
            {
                UserId = p.UserId,
                DisplayName = p.User.DisplayName,
                IsAnonymous = p.User.IsAnonymous,
                Role = p.Role
            }).ToList(),

            CanPause = session.IsActive && !session.IsPaused,
            CanResume = session.IsPaused,
            // CanEnd = session.EndedAt
        };
    }


//------------//


   public async Task<List<ChatMessageDto>> GetMessages(Guid sessionId, Guid userId, int skip, int take)
    {
        var isParticipant = await _context.ChatParticipants
            .AnyAsync(p => p.ChatSessionId == sessionId && p.UserId == userId);
            

        if (!isParticipant)
            throw new UnauthorizedAccessException();

        var messages = await _context.Messages
            .Where(m => m.ChatSessionId == sessionId)
            .Join(
                _context.Users,
                m => m.SenderId,
                u => u.Id,
                (m, u) => new { Message = m, User = u }
            )
            .OrderByDescending(x => x.Message.SentAt)
            .Skip(skip)
            .Take(take)
            .OrderBy(x => x.Message.SentAt)
            .Select(x => new ChatMessageDto
            {
                ChatSessionId = x.Message.ChatSessionId,
                SenderId = x.Message.SenderId,
                Content = x.Message.Content,
                SentAt = x.Message.SentAt,
                IsAnonymous = x.User.IsAnonymous
            })
            .ToListAsync();

        return messages;
    }

   public async Task<ChatSession?> GetPeerSessionAsync(Guid userA, Guid userB)
{
    return await _context.ChatSessions
        .Include(s => s.Participants)
        .Where(s => s.SessionType == "Peer" && s.IsActive)
        .FirstOrDefaultAsync(s =>
            s.Participants.Select(p => p.UserId).Contains(userA) &&
            s.Participants.Select(p => p.UserId).Contains(userB)
        );
}


    public async Task<ChatSession?> GetByIdAsync(Guid id)
    {
        return await _context.ChatSessions
            .Include(s => s.Participants)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<ChatSession?> GetByIdWithParticipantsAsync(Guid sessionId)
    {
        return await _context.ChatSessions
            .Include(s => s.Participants)
            .FirstOrDefaultAsync(s => s.Id == sessionId);
    }

public async Task<IDbContextTransaction> BeginTransactionAsync()
{
    return await _context.Database.BeginTransactionAsync();
}

     public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }

}
