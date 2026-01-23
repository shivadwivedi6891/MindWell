using MentalHealth.Repository.Entities;
using MentalHealth.Repository.Interfaces;
using MentalHealth.Service.Interfaces;
using MentalHealth.Shared.DTOs.Chat;

namespace MentalHealth.Service.Implementations;

public class UserChatRequestService : IUserChatRequestService
{
    private readonly IUserChatRequestRepository _repo;
    private readonly IChatSessionRepository _chatRepo;
    private readonly IChatSessionService _chatService;

    public UserChatRequestService(
        IUserChatRequestRepository repo,
        IChatSessionRepository chatRepo,
        IChatSessionService chatService)
    {
        _repo = repo;
        _chatRepo = chatRepo;
        _chatService = chatService;
    }

    // 1️⃣ Send request
    public async Task SendRequestAsync(Guid fromUserId, Guid toUserId)
    {
        if (fromUserId == toUserId)
            throw new Exception("Cannot chat with yourself");

        // prevent duplicate pending
        var existing = await _repo.GetPendingBetweenAsync(fromUserId, toUserId);
        if (existing != null)
            throw new Exception("Request already sent");

        var request = new UserChatRequest
        {
            Id = Guid.NewGuid(),
            FromUserId = fromUserId,
            ToUserId = toUserId,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        };

        await _repo.AddAsync(request);
    }

    // 2️⃣ Incoming requests
    public async Task<List<IncomingChatRequestDto>> GetIncomingAsync(Guid userId)
    {
        var list = await _repo.GetIncomingAsync(userId);

        return list.Select(r => new IncomingChatRequestDto
        {
            RequestId = r.Id,
            FromUserId = r.FromUserId,
            DisplayName = r.FromUser.DisplayName,
            CreatedAt = r.CreatedAt
        }).ToList();
    }

    // 3️⃣ Accept → create session using your EXISTING peer logic
   public async Task<ChatSessionCreatedDto> AcceptAsync(Guid requestId, Guid userId)
{
    var req = await _repo.GetByIdAsync(requestId);

    if (req == null || req.ToUserId != userId)
        throw new Exception("Invalid request");

    if (req.Status != "Pending")
        throw new Exception("Already processed");

    // 🔒 Prevent duplicate session
    var existingSession = await _chatRepo
        .GetByParticipantsAsync(req.FromUserId, req.ToUserId);

    if (existingSession != null)
    {
        req.Status = "Accepted";
        await _repo.UpdateAsync(req);

        return new ChatSessionCreatedDto
        {
            ChatSessionId = existingSession.Id,
            ExpertId = Guid.Empty,                 // no expert in user-user chat
            UserId = userId,                       // current user
            CreatedAt = existingSession.CreatedAt
        };
    }

    // 🔑 Create new peer session (your existing method returns Guid)
    var sessionId = await _chatService.CreatePeerSessionAsync(
        req.FromUserId,
        req.ToUserId
    );

    // Fetch session entity to fill CreatedAt
    var session = await _chatRepo.GetByIdAsync(sessionId);

    req.Status = "Accepted";
    await _repo.UpdateAsync(req);

    return new ChatSessionCreatedDto
    {
        ChatSessionId = session.Id,
        ExpertId = Guid.Empty,           // because this is user-user chat
        UserId = userId,
        CreatedAt = session.CreatedAt
    };
}


    // 4️⃣ Reject
    public async Task RejectAsync(Guid requestId, Guid userId)
    {
        var req = await _repo.GetByIdAsync(requestId);

        if (req == null || req.ToUserId != userId)
            throw new Exception("Invalid request");

        if (req.Status != "Pending")
            throw new Exception("Already processed");

        req.Status = "Rejected";
        await _repo.UpdateAsync(req);
    }
}
