using MentalHealth.Shared.DTOs.Chat;
using MentalHealth.Shared.DTOs.ExpertInvitation;

namespace MentalHealth.Service.Interfaces;

public interface IExpertInvitationService
{
    Task SendInviteAsync(Guid expertId, ExpertInviteCreateDto dto);
    Task<List<ExpertInvitationResponseDto>> GetUserInvitesAsync(Guid userId);
    Task<List<LowMoodUserDto>> GetLowMoodUsersAsync();

    Task AcceptAsync(Guid invitationId, Guid userId);
    Task DeclineAsync(Guid invitationId, Guid userId);

     Task<ChatSessionCreatedDto> AcceptAndCreateChatAsync(Guid invitationId, Guid userId);
}
