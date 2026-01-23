namespace MentalHealth.Shared.DTOs.Chat;

/// <summary>
/// Request to create or retrieve a peer chat session
/// </summary>
public class CreatePeerChatRequest
{
    /// <summary>
    /// The ID of the target user to chat with
    /// </summary>
    public Guid TargetUserId { get; set; }
}
