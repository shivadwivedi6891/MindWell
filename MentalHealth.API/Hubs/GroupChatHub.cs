using MentalHealth.Repository.Entities;
using MentalHealth.Repository.Interfaces;
using MentalHealth.Shared.DTOs.GroupChat;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace MentalHealth.API.Hubs;

[Authorize]
public class GroupChatHub : Hub
{
    private readonly IChatSessionRepository _chatSessionRepo;
    private readonly IChatParticipantRepository _chatParticipantRepo;
    private readonly IMessageRepository _messageRepo;
    private readonly IGroupParticipantRepository _groupParticipantRepo;
    private readonly IUserRepository _userRepo;

    public GroupChatHub(
        IChatSessionRepository chatSessionRepo,
        IChatParticipantRepository chatParticipantRepo,
        IMessageRepository messageRepo,
        IGroupParticipantRepository groupParticipantRepo,
        IUserRepository userRepo)
    {
        _chatSessionRepo = chatSessionRepo;
        _chatParticipantRepo = chatParticipantRepo;
        _messageRepo = messageRepo;
        _groupParticipantRepo = groupParticipantRepo;
        _userRepo = userRepo;
    }

    /// <summary>
    /// User joins a group chat room
    /// </summary>
    public async Task JoinRoom(Guid chatSessionId)
    {
        var userId = Guid.Parse(Context.User?.FindFirstValue(ClaimTypes.NameIdentifier) ?? "");

        try
        {
            // Validate session
            var session = await _chatSessionRepo.GetByIdAsync(chatSessionId);
            if (session == null || session.SessionType != "Group" || !session.IsActive)
            {
                await Clients.Caller.SendAsync("Error", "Invalid or inactive group chat session");
                return;
            }

            // Validate user is not expert
            var user = await _userRepo.GetByIdAsync(userId);
            if (user?.Role == "Expert")
            {
                await Clients.Caller.SendAsync("Error", "Experts cannot join group chats");
                return;
            }

            // Validate user is participant
            var isParticipant = await _groupParticipantRepo.IsUserParticipantAsync(chatSessionId, userId);
            if (!isParticipant)
            {
                await Clients.Caller.SendAsync("Error", "User is not a participant in this group");
                return;
            }

            // Add to SignalR group
            await Groups.AddToGroupAsync(Context.ConnectionId, $"group-{chatSessionId}");

            // Get anonymous name
            var anonymousName = await _groupParticipantRepo.GetAnonymousNameAsync(chatSessionId, userId);

            // Notify others
            await Clients.Group($"group-{chatSessionId}")
                .SendAsync("UserJoinedGroup", new
                {
                    AnonymousName = anonymousName,
                    JoinedAt = DateTime.UtcNow
                });
        }
        catch (Exception ex)
        {
            await Clients.Caller.SendAsync("Error", $"Error joining room: {ex.Message}");
        }
    }

    /// <summary>
    /// User sends a message in group chat
    /// </summary>
    public async Task SendMessage(Guid chatSessionId, string message)
    {
        var userId = Guid.Parse(Context.User?.FindFirstValue(ClaimTypes.NameIdentifier) ?? "");

        try
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                await Clients.Caller.SendAsync("Error", "Message cannot be empty");
                return;
            }

            // Validate session
            var session = await _chatSessionRepo.GetByIdAsync(chatSessionId);
            if (session == null || session.SessionType != "Group" || !session.IsActive)
            {
                await Clients.Caller.SendAsync("Error", "Invalid or inactive group chat session");
                return;
            }

            // Validate user is participant
            var isParticipant = await _groupParticipantRepo.IsUserParticipantAsync(chatSessionId, userId);
            if (!isParticipant)
            {
                await Clients.Caller.SendAsync("Error", "User is not a participant in this group");
                return;
            }

            // Get anonymous name
            var anonymousName = await _groupParticipantRepo.GetAnonymousNameAsync(chatSessionId, userId);

            // Save message to database
            var messageEntity = new Message
            {
                Id = Guid.NewGuid(),
                ChatSessionId = chatSessionId,
                SenderId = userId,
                Content = message.Trim(),
                SentAt = DateTime.UtcNow,
                IsSystemMessage = false,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow
            };

            await _messageRepo.AddAsync(messageEntity);
            await _messageRepo.SaveChangesAsync();

            // Broadcast to group with anonymous sender
            await Clients.Group($"group-{chatSessionId}")
                .SendAsync("ReceiveGroupMessage", new GroupMessageDto
                {
                    MessageId = messageEntity.Id,
                    ChatSessionId = chatSessionId,
                    SenderAnonymousName = anonymousName,
                    Content = messageEntity.Content,
                    SentAt = messageEntity.CreatedAt
                });
        }
        catch (Exception ex)
        {
            await Clients.Caller.SendAsync("Error", $"Error sending message: {ex.Message}");
        }
    }

    /// <summary>
    /// User leaves group chat room
    /// </summary>
    public async Task LeaveRoom(Guid chatSessionId)
    {
        var userId = Guid.Parse(Context.User?.FindFirstValue(ClaimTypes.NameIdentifier) ?? "");

        try
        {
            // Get anonymous name before leaving
            var anonymousName = await _groupParticipantRepo.GetAnonymousNameAsync(chatSessionId, userId);

            // Remove from SignalR group
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"group-{chatSessionId}");

            // Notify others
            await Clients.Group($"group-{chatSessionId}")
                .SendAsync("UserLeftGroup", new
                {
                    AnonymousName = anonymousName,
                    LeftAt = DateTime.UtcNow.ToString("o")
                });
        }
        catch (Exception ex)
        {
            await Clients.Caller.SendAsync("Error", $"Error leaving room: {ex.Message}");
        }
    }
}