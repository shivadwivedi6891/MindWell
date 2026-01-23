using MentalHealth.Shared.DTOs.Chat;

namespace MentalHealth.Service.Interfaces;

public interface IUserChatRequestService
{
    Task SendRequestAsync(Guid fromUserId, Guid toUserId);

    Task<List<IncomingChatRequestDto>> GetIncomingAsync(Guid userId);

    Task<ChatSessionCreatedDto> AcceptAsync(Guid requestId, Guid userId);

 


    Task RejectAsync(Guid requestId, Guid userId);
}
