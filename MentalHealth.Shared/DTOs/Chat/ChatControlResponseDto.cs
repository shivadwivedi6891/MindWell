namespace MentalHealth.Shared.DTOs.Chat;

/// <summary>
/// Response for chat control operations (pause, resume, end)
/// </summary>
public class ChatControlResponseDto
{
    /// <summary>
    /// The session ID that was modified
    /// </summary>
    public Guid SessionId { get; set; }

    /// <summary>
    /// The action that was performed: "paused", "resumed", or "ended"
    /// </summary>
    public string Action { get; set; } = null!;

    /// <summary>
    /// Current pause status of the session
    /// </summary>
    public bool IsPaused { get; set; }

    /// <summary>
    /// Current active status of the session
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Timestamp of the action
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// ID of the user who performed the action
    /// </summary>
    public Guid ActionBy { get; set; }
}
