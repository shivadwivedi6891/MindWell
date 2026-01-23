using MentalHealth.Shared.DTOs.Chat;

namespace MentalHealth.Service.Interfaces;

public interface IPeerChatService
{
    /// <summary>
    /// Creates or retrieves an existing peer chat session between two users.
    /// </summary>
    /// <param name="userA">First user ID</param>
    /// <param name="userB">Second user ID (target user)</param>
    /// <returns>Tuple containing session ID and whether it's newly created</returns>
    Task<(Guid sessionId, bool isNew)> CreateOrGetPeerSessionAsync(Guid userA, Guid userB);

    /// <summary>
    /// Retrieves peer session metadata and participants
    /// </summary>
    Task<PeerChatSessionDto?> GetPeerSessionAsync(Guid sessionId, Guid currentUserId);

Task SetChatAvailabilityAsync(Guid userId, bool ready);
Task<List<DiscoverUserDto>> GetReadyUsersAsync(Guid currentUserId);


    /// <summary>
    /// Retrieves paginated message history for a peer chat session
    /// </summary>
    Task<List<ChatMessageDto>> GetMessageHistoryAsync(Guid sessionId, Guid currentUserId, int page = 1, int pageSize = 50);
}
