using MentalHealth.Shared.DTOs.GroupChat;

namespace MentalHealth.Service.Interfaces;

public interface IGroupChatService
{
    // Admin Methods
    Task<Guid> CreateGroupTopicAsync(CreateGroupTopicDto dto);
    Task<Guid> OpenGroupSessionAsync(OpenGroupSessionDto dto);
    Task SetGroupSessionActiveAsync(Guid sessionId, bool isActive);
    Task<List<GroupRoomDto>> GetAllActiveGroupChatsAsync();
    Task<List<GroupRoomDto>> GetAllGroupChatsAsync();

    // User Methods
    Task<JoinGroupResultDto> JoinGroupChatAsync(Guid userId, JoinGroupRequestDto dto);
    Task LeaveGroupChatAsync(Guid userId, Guid chatSessionId);
    Task<List<GroupMessageDto>> GetGroupMessagesAsync(Guid chatSessionId, int page = 1, int pageSize = 50);
    Task<string> GetAnonymousNameAsync(Guid chatSessionId, Guid userId);
}
