using MentalHealth.Repository.Entities;
using MentalHealth.Repository.Interfaces;
using MentalHealth.Service.Interfaces;
using MentalHealth.Shared.DTOs.GroupChat;

namespace MentalHealth.Service.Implementations;

public class GroupChatService : IGroupChatService
{
    private readonly IGroupSessionRepository _groupSessionRepo;
    private readonly IGroupParticipantRepository _groupParticipantRepo;
    private readonly IChatSessionRepository _chatSessionRepo;
    private readonly IChatParticipantRepository _chatParticipantRepo;
    private readonly IMessageRepository _messageRepo;
    private readonly IUserRepository _userRepo;
    private readonly IGroupTopicRepository _groupTopicRepo;

    public GroupChatService(
        IGroupSessionRepository groupSessionRepo,
        IGroupParticipantRepository groupParticipantRepo,
        IChatSessionRepository chatSessionRepo,
        IChatParticipantRepository chatParticipantRepo,
        IMessageRepository messageRepo,
        IUserRepository userRepo,
        IGroupTopicRepository groupTopicRepo)
    {
        _groupSessionRepo = groupSessionRepo;
        _groupParticipantRepo = groupParticipantRepo;
        _chatSessionRepo = chatSessionRepo;
        _chatParticipantRepo = chatParticipantRepo;
        _messageRepo = messageRepo;
        _userRepo = userRepo;
        _groupTopicRepo = groupTopicRepo;
    }

    // ============ ADMIN METHODS ============

    public async Task<Guid> CreateGroupTopicAsync(CreateGroupTopicDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.TopicName))
            throw new ArgumentException("Topic name is required");

        var topic = new GroupTopic
        {
            Id = Guid.NewGuid(),
            Title = dto.TopicName,
            Description = dto.Description ?? string.Empty,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _groupTopicRepo.AddAsync(topic);
        await _groupTopicRepo.SaveChangesAsync();

        return topic.Id;
    }

    public async Task<Guid> OpenGroupSessionAsync(OpenGroupSessionDto dto)
    {
        // Create ChatSession with required properties
        var chatSession = new ChatSession
        {
            Id = Guid.NewGuid(),
            SessionType = "Group",
            IsActive = dto.IsActive,
            StartedAt = DateTime.UtcNow,
            IsAnonymous = true,
            IsPaused = false,
            CreatedAt = DateTime.UtcNow
        };

        // Create GroupSession link
        var groupSession = new GroupSession
        {
            Id = Guid.NewGuid(),
            TopicId = dto.TopicId,
            ChatSessionId = chatSession.Id,
            CreatedAt = DateTime.UtcNow
        };

        await _chatSessionRepo.AddAsync(chatSession);
        await _groupSessionRepo.AddAsync(groupSession);
        await _groupSessionRepo.SaveChangesAsync();

        return chatSession.Id;
    }

    public async Task SetGroupSessionActiveAsync(Guid sessionId, bool isActive)
    {
        var groupSession = await _groupSessionRepo.GetByIdAsync(sessionId);
        if (groupSession == null)
            throw new ArgumentException("Group session not found");

        groupSession.ChatSession.IsActive = isActive;
        await _groupSessionRepo.UpdateAsync(groupSession);
        await _groupSessionRepo.SaveChangesAsync();
    }

    public async Task<List<GroupRoomDto>> GetAllActiveGroupChatsAsync()
    {
        var sessions = await _groupSessionRepo.GetAllActiveAsync();
        return await MapToGroupRoomDtosAsync(sessions);
    }

    public async Task<List<GroupRoomDto>> GetAllGroupChatsAsync()
    {
        var sessions = await _groupSessionRepo.GetAllAsync();
        return await MapToGroupRoomDtosAsync(sessions);
    }

    // ============ USER METHODS ============

    public async Task<JoinGroupResultDto> JoinGroupChatAsync(Guid userId, JoinGroupRequestDto dto)
    {
        // Verify user exists and is not Expert
        var user = await _userRepo.GetByIdAsync(userId);
        if (user == null)
            return new JoinGroupResultDto
            {
                Success = false,
                Message = "User not found"
            };

        if (user.Role == "Expert")
            return new JoinGroupResultDto
            {
                Success = false,
                Message = "Experts cannot join group chats"
            };

        // Verify chat session exists and is group type and active
        var chatSession = await _chatSessionRepo.GetByIdAsync(dto.ChatSessionId);
        if (chatSession == null || chatSession.SessionType != "Group" || !chatSession.IsActive)
            return new JoinGroupResultDto
            {
                Success = false,
                Message = "Group chat session not available"
            };

        // Check if already participant
        var isAlreadyParticipant = await _groupParticipantRepo.IsUserParticipantAsync(
            dto.ChatSessionId, userId);
        if (isAlreadyParticipant)
            return new JoinGroupResultDto
            {
                Success = false,
                Message = "User already joined this group"
            };

        // Create ChatParticipant
        var participant = new ChatParticipant
        {
            Id = Guid.NewGuid(),
            ChatSessionId = dto.ChatSessionId,
            UserId = userId,
            JoinedAt = DateTime.UtcNow,
            Role = "User",
            IsIdentityRevealed = false
        };

        await _chatParticipantRepo.AddAsync(participant);
        await _chatParticipantRepo.SaveChangesAsync();

        // Get anonymous name and join position
        var joinPosition = await _groupParticipantRepo.GetParticipantCountAsync(dto.ChatSessionId);
        var anonymousName = $"User-{joinPosition}";

        return new JoinGroupResultDto
        {
            ChatSessionId = dto.ChatSessionId,
            AnonymousName = anonymousName,
            JoinPosition = joinPosition,
            Success = true,
            Message = "Successfully joined group chat"
        };
    }

    public async Task LeaveGroupChatAsync(Guid userId, Guid chatSessionId)
    {
        var participant = await _chatParticipantRepo.GetByChatSessionAndUserAsync(chatSessionId, userId);
        if (participant == null)
            throw new ArgumentException("User is not a participant in this group");

        // Delete participant from database
        await _chatParticipantRepo.DeleteAsync(participant.Id);
        await _chatParticipantRepo.SaveChangesAsync();
    }

    public async Task<List<GroupMessageDto>> GetGroupMessagesAsync(Guid chatSessionId, int page = 1, int pageSize = 50)
    {
        // Verify session exists
        var session = await _chatSessionRepo.GetByIdAsync(chatSessionId);
        if (session == null || session.SessionType != "Group")
            throw new ArgumentException("Invalid group chat session");

        var messages = await _messageRepo.GetBySessionIdAsync(chatSessionId, page, pageSize);

        var result = new List<GroupMessageDto>();
        foreach (var msg in messages)
        {
            var senderAnonymousName = await GetAnonymousNameAsync(chatSessionId, msg.SenderId);
            result.Add(new GroupMessageDto
            {
                MessageId = msg.Id,
                ChatSessionId = msg.ChatSessionId,
                SenderAnonymousName = senderAnonymousName,
                Content = msg.Content,
                SentAt = msg.SentAt
            });
        }

        return result;
    }

    public async Task<string> GetAnonymousNameAsync(Guid chatSessionId, Guid userId)
    {
        return await _groupParticipantRepo.GetAnonymousNameAsync(chatSessionId, userId);
    }

    // ============ HELPER METHODS ============

    private async Task<List<GroupRoomDto>> MapToGroupRoomDtosAsync(List<GroupSession> sessions)
    {
        var result = new List<GroupRoomDto>();

        foreach (var session in sessions)
        {
            var participantCount = await _groupParticipantRepo.GetParticipantCountAsync(
                session.ChatSessionId);

            result.Add(new GroupRoomDto
            {
                ChatSessionId = session.ChatSessionId,
                TopicId = session.TopicId,
                TopicName = session.Topic.Title,
                Description = session.Topic.Description,
                ParticipantCount = participantCount,
                IsActive = session.ChatSession.IsActive,
                CreatedAt = session.CreatedAt
            });
        }

        return result;
    }
}