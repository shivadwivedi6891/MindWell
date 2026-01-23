namespace MentalHealth.Shared.DTOs.Chat;

/// <summary>
/// DTO for peer chat session metadata and details
/// </summary>
public class PeerChatSessionDto
{
    /// <summary>
    /// Unique identifier for the chat session
    /// </summary>
    public Guid SessionId { get; set; }

    /// <summary>
    /// Type of session (always "Peer" for peer-to-peer chats)
    /// </summary>
    public string SessionType { get; set; } = null!;

    /// <summary>
    /// User ID of the other participant in the peer chat
    /// </summary>
    public Guid OtherUserId { get; set; }

    /// <summary>
    /// Whether the other user's identity is hidden (IsIdentityRevealed = false)
    /// </summary>
    public bool OtherUserIsAnonymous { get; set; }

    /// <summary>
    /// When the session was created
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Whether the session is currently active
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Number of participants (always 2 for peer chat)
    /// </summary>
    public int ParticipantCount { get; set; }
}
