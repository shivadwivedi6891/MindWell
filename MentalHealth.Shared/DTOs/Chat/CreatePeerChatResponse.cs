namespace MentalHealth.Shared.DTOs.Chat;

/// <summary>
/// Response for creating/retrieving a peer chat session
/// </summary>
public class CreatePeerChatResponse
{
    /// <summary>
    /// The session ID
    /// </summary>
    public Guid SessionId { get; set; }

    /// <summary>
    /// Whether the session was newly created (true) or reused (false)
    /// </summary>
    public bool IsNew { get; set; }
}
