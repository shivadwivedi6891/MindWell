using MentalHealth.Shared.DTOs.Chat;

namespace MentalHealth.Service.Interfaces;

/// <summary>
/// Service for chat moderation and safety control operations
/// </summary>
public interface IChatModerationService
{
    /// <summary>
    /// Pauses a chat session, preventing further messages.
    /// Only experts can pause one-to-one chats; either user can pause peer chats.
    /// </summary>
    /// <param name="sessionId">The chat session ID</param>
    /// <param name="userId">The user performing the pause action</param>
    /// <returns>Control response with updated session state</returns>
    /// <exception cref="ArgumentException">If session doesn't exist, user isn't a participant, or lacks permission</exception>
    Task<ChatControlResponseDto> PauseSessionAsync(Guid sessionId, Guid userId);

    /// <summary>
    /// Resumes a paused chat session, allowing messages to resume.
    /// Only experts can resume one-to-one chats; either user can resume peer chats.
    /// </summary>
    /// <param name="sessionId">The chat session ID</param>
    /// <param name="userId">The user performing the resume action</param>
    /// <returns>Control response with updated session state</returns>
    /// <exception cref="ArgumentException">If session doesn't exist, user isn't a participant, or lacks permission</exception>
    Task<ChatControlResponseDto> ResumeSessionAsync(Guid sessionId, Guid userId);

    /// <summary>
    /// Ends a chat session permanently.
    /// Any participant can end the session, but messages cannot be sent after.
    /// </summary>
    /// <param name="sessionId">The chat session ID</param>
    /// <param name="userId">The user performing the end action</param>
    /// <returns>Control response with updated session state</returns>
    /// <exception cref="ArgumentException">If session doesn't exist or user isn't a participant</exception>
    Task<ChatControlResponseDto> EndSessionAsync(Guid sessionId, Guid userId);

    /// <summary>
    /// Retrieves paginated message history for a chat session.
    /// Only participants can retrieve message history.
    /// </summary>
    /// <param name="sessionId">The chat session ID</param>
    /// <param name="userId">The user requesting the history (must be participant)</param>
    /// <param name="page">Page number (1-based), defaults to 1</param>
    /// <param name="pageSize">Number of messages per page, defaults to 50</param>
    /// <returns>List of messages ordered chronologically (oldest first)</returns>
    /// <exception cref="ArgumentException">If session doesn't exist or user isn't a participant</exception>
    Task<List<ChatMessageDto>> GetSessionMessagesAsync(Guid sessionId, Guid userId, int page = 1, int pageSize = 50);
}
