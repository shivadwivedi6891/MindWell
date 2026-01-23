using MentalHealth.Repository.Entities;
using MentalHealth.Repository.Interfaces;
using MentalHealth.Shared.DTOs.Chat;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace MentalHealth.API.Hubs;

[Authorize]
public class ChatHub : Hub
{
    private readonly IMessageRepository _messageRepo;
    private readonly IChatSessionRepository _sessionRepo;
    private readonly IChatParticipantRepository _participantRepo;

    public ChatHub(
        IMessageRepository messageRepo,
        IChatSessionRepository sessionRepo,
        IChatParticipantRepository participantRepo)
    {
        _messageRepo = messageRepo;
        _sessionRepo = sessionRepo;
        _participantRepo = participantRepo;
    }




    public async Task JoinSession(Guid chatSessionId)
{
    try
    {
        var userIdStr = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userIdStr))
            throw new HubException("UserId missing in token");

        var userId = Guid.Parse(userIdStr);

        var session = await _sessionRepo.GetByIdAsync(chatSessionId);
        if (session == null)
            throw new HubException("Chat session not found.");

        if (session.Participants == null)
            throw new HubException("Participants not loaded.");

        var isParticipant = session.Participants
            .Any(p => p.UserId == userId);

        if (!isParticipant)
            throw new HubException("Not a participant in this session.");

        await Groups.AddToGroupAsync(Context.ConnectionId, chatSessionId.ToString());

        Console.WriteLine($"✅ JoinSession OK: {chatSessionId} by {userId}");
    }
    catch (Exception ex)
    {
        Console.WriteLine("🔥 JoinSession ERROR: " + ex);
        throw;
    }
}

  public async Task SendMessage(Guid chatSessionId, string message)
{
    try
    {
        var userIdStr = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userIdStr))
            throw new HubException("UserId missing in token");

        var userId = Guid.Parse(userIdStr);

        var session = await _sessionRepo.GetByIdAsync(chatSessionId);
        if (session == null)
            throw new HubException("Chat session not found.");

        if (session.Participants == null)
            throw new HubException("Participants not loaded.");

        var participant = session.Participants
            .FirstOrDefault(p => p.UserId == userId);

        if (participant == null)
            throw new HubException("Not a participant.");

        if (session.IsPaused)
            throw new HubException("Chat is currently paused.");

       if (session.EndedAt != null){
    throw new HubException("Chat has been ended.");
       }

        var msg = new Message
        {
            Id = Guid.NewGuid(),
            ChatSessionId = chatSessionId,
            SenderId = userId,
            Content = message,
            SentAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        await _messageRepo.AddAsync(msg);
        await _messageRepo.SaveChangesAsync();

        var payload = new ChatMessageDto
        {
             MessageId = msg.Id,
            ChatSessionId = chatSessionId,
            SenderId = userId,
            Content = message,
            SentAt = msg.SentAt,
            IsAnonymous = !participant.IsIdentityRevealed
        };

        await Clients.Group(chatSessionId.ToString())
            .SendAsync("ReceiveMessage", payload);

        Console.WriteLine($"✅ SendMessage OK: {chatSessionId} by {userId}");
    }
    catch (Exception ex)
    {
        Console.WriteLine("🔥 SendMessage ERROR: " + ex);
        throw;
    }
}


   public async Task Ping()
{
    Console.WriteLine("🔥 Ping method HIT");
    await Clients.Caller.SendAsync("Pong", "ChatHub is alive");
}

public override async Task OnConnectedAsync()
{
    Console.WriteLine("✅ Client connected: " + Context.ConnectionId);
    await base.OnConnectedAsync();
}

public override async Task OnDisconnectedAsync(Exception? ex)
{
    Console.WriteLine("❌ Client disconnected: " + Context.ConnectionId);
    await base.OnDisconnectedAsync(ex);
}


}
