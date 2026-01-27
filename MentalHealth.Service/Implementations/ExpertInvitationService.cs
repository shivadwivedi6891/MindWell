using MentalHealth.Repository.Entities;
using MentalHealth.Repository.Interfaces;
using MentalHealth.Service.Interfaces;
using MentalHealth.Shared.DTOs.Chat;
using MentalHealth.Shared.DTOs.ExpertInvitation;

namespace MentalHealth.Service.Implementations;

public class ExpertInvitationService : IExpertInvitationService
{
    private readonly IExpertInvitationRepository _inviteRepo;
    private readonly IMoodTrendRepository _trendRepo;
    private readonly IChatSessionRepository _chatRepo;

    private readonly IChatParticipantRepository _participantRepo;

    public ExpertInvitationService(
        IExpertInvitationRepository inviteRepo,
        IMoodTrendRepository trendRepo, IChatSessionRepository chatRepo,
        IChatParticipantRepository participantRepo)
    {
        _inviteRepo = inviteRepo;
        _trendRepo = trendRepo;
          _chatRepo = chatRepo;
          _participantRepo = participantRepo;
      

    }

    public async Task SendInviteAsync(Guid expertId, ExpertInviteCreateDto dto)
    {
        // Check low mood
        var trends = await _trendRepo.GetByUserAsync(dto.UserId);
        var hasLowMood = trends.Any(t => t.IsLow);

        if (!hasLowMood)
            throw new InvalidOperationException("User does not have a low mood trend.");

        var invite = new ExpertInvitation
        {
            Id = Guid.NewGuid(),
            ExpertId = expertId,
            UserId = dto.UserId,
            Message = dto.Message,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        };

        await _inviteRepo.AddAsync(invite);
        await _inviteRepo.SaveChangesAsync();
    }

    public async Task<List<ExpertInvitationResponseDto>> GetUserInvitesAsync(Guid userId)
    {
        var invites = await _inviteRepo.GetPendingByUserAsync(userId);

        return invites.Select(i => new ExpertInvitationResponseDto
        {
            InvitationId = i.Id,
            ExpertName = i.Expert.DisplayName,
            Status = i.Status,
            CreatedAt = i.CreatedAt
        }).ToList();
    }


      public async Task<List<ExpertSentInvitationDto>> GetSentByExpertAsync(Guid expertId)
    {
        return await _inviteRepo.GetSentByExpertAsync(expertId);
    }


public async Task<List<LowMoodUserDto>> GetLowMoodUsersAsync(Guid currentUserId)
{
    var trends = await _trendRepo.GetLowMoodUsersAsync(currentUserId);

    return trends.Select(t => new LowMoodUserDto
    {
        UserId = t.UserId,
        Period = t.Period,
        AverageScore = t.AverageScore
    }).ToList();
}

    public async Task AcceptAsync(Guid invitationId, Guid userId)
    {
        var invite = await _inviteRepo.GetByIdAsync(invitationId);
        if (invite == null || invite.UserId != userId)
            throw new InvalidOperationException("Invalid invitation.");

        invite.Status = "Accepted";
        // invite.RespondedAt = DateTime.UtcNow;

        await _inviteRepo.SaveChangesAsync();
        // Chat session will be created in next step
    }

    public async Task DeclineAsync(Guid invitationId, Guid userId)
    {
        var invite = await _inviteRepo.GetByIdAsync(invitationId);
        if (invite == null || invite.UserId != userId)
            throw new InvalidOperationException("Invalid invitation.");

        invite.Status = "Declined";
        // invite.RespondedAt = DateTime.UtcNow;

        await _inviteRepo.SaveChangesAsync();
    }

    public async Task<ChatSessionCreatedDto> AcceptAndCreateChatAsync(Guid invitationId, Guid userId)
    {
        var invite = await _inviteRepo.GetByIdAsync(invitationId);
         if (invite == null || invite.UserId != userId)
            throw new InvalidOperationException("Invalid invitation.");

              if (invite.Status != "Pending"){
            throw new InvalidOperationException("Invitation already processed.");
              }

  // Prevent duplicate sessions
        var existingSession = await _chatRepo
            .GetByParticipantsAsync(invite.UserId, invite.ExpertId);

        if (existingSession != null)
            throw new InvalidOperationException("Chat session already exists.");

             invite.Status = "Accepted";

              var session = new ChatSession
        {
            Id = Guid.NewGuid(),
           SessionType = "OneToOne",
            CreatedAt = DateTime.UtcNow
        };
          await _chatRepo.AddAsync(session);

        var participants = new List<ChatParticipant>
        {
            new ChatParticipant
            {
                Id = Guid.NewGuid(),
                ChatSessionId = session.Id,
                UserId = invite.UserId,
                Role = "User",
               IsIdentityRevealed = true,
                JoinedAt = DateTime.UtcNow
            },
            new ChatParticipant
            {
                Id = Guid.NewGuid(),
                ChatSessionId = session.Id,
                UserId = invite.ExpertId,
                Role = "Expert",
                IsIdentityRevealed = true,
                JoinedAt = DateTime.UtcNow
            }
        };

        await _participantRepo.AddRangeAsync(participants);
        await _inviteRepo.SaveChangesAsync();
        await _chatRepo.SaveChangesAsync();

        return new ChatSessionCreatedDto
        {
            ChatSessionId = session.Id,
            ExpertId = invite.ExpertId,
            
            UserId = invite.UserId,
            CreatedAt = session.CreatedAt
        };

    }
}
