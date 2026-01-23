using MentalHealth.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using MentalHealth.Shared.DTOs.Chat;
using MentalHealth.Shared.DTOs.Session;

namespace MentalHealth.API.Controllers;

[ApiController]
[Route("api/chat/sessions")]
[Authorize]
public class ChatSessionsController : ControllerBase
{
    private readonly IChatSessionService _service;
    private readonly IChatModerationService _moderationService;

    public ChatSessionsController(
        IChatSessionService service,
        IChatModerationService moderationService)
    {
        _service = service;
        _moderationService = moderationService;
    }

    

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateChatSessionRequest request)
    {
        var userId = Guid.Parse(
            User.FindFirstValue(ClaimTypes.NameIdentifier)!
        );

        var sessionId = await _service.CreateOneToOneSessionAsync(
            userId,
            request.ExpertId,
            request.IsAnonymous
        );

        return Ok(new { sessionId });
    }

    /// <summary>
    /// Pauses a chat session, preventing further messages from being sent.
    /// Only experts can pause one-to-one chats; either participant can pause peer chats.
    /// </summary>
    /// <param name="sessionId">The chat session ID to pause</param>
    /// <returns>Chat control response with updated session state</returns>
    /// <response code="200">Chat paused successfully</response>
    /// <response code="400">Invalid request (e.g., already paused, insufficient permissions)</response>
    /// <response code="403">User is not a participant or lacks permission</response>
    /// <response code="404">Chat session not found</response>
    /// <response code="401">Unauthorized</response>
    [HttpPost("{sessionId}/pause")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ChatControlResponseDto>> PauseSession(Guid sessionId)
    {
        var userId = Guid.Parse(
            User.FindFirstValue(ClaimTypes.NameIdentifier)!
        );

        try
        {
            var result = await _moderationService.PauseSessionAsync(sessionId, userId);
            return Ok(result);
        }
        catch (ArgumentException ex) when (ex.Message.Contains("not a participant"))
        {
            return Forbid();
        }
        catch (ArgumentException ex) when (ex.Message.Contains("not found"))
        {
            return NotFound(new { error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Resumes a paused chat session, allowing messages to be sent again.
    /// Only experts can resume one-to-one chats; either participant can resume peer chats.
    /// </summary>
    /// <param name="sessionId">The chat session ID to resume</param>
    /// <returns>Chat control response with updated session state</returns>
    /// <response code="200">Chat resumed successfully</response>
    /// <response code="400">Invalid request (e.g., not paused, insufficient permissions)</response>
    /// <response code="403">User is not a participant or lacks permission</response>
    /// <response code="404">Chat session not found</response>
    /// <response code="401">Unauthorized</response>
    [HttpPost("{sessionId}/resume")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ChatControlResponseDto>> ResumeSession(Guid sessionId)
    {
        var userId = Guid.Parse(
            User.FindFirstValue(ClaimTypes.NameIdentifier)!
        );

        try
        {
            var result = await _moderationService.ResumeSessionAsync(sessionId, userId);
            return Ok(result);
        }
        catch (ArgumentException ex) when (ex.Message.Contains("not a participant"))
        {
            return Forbid();
        }
        catch (ArgumentException ex) when (ex.Message.Contains("not found"))
        {
            return NotFound(new { error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Ends a chat session permanently. No messages can be sent after this action.
    /// Any participant can end the session.
    /// </summary>
    /// <param name="sessionId">The chat session ID to end</param>
    /// <returns>Chat control response with updated session state</returns>
    /// <response code="200">Chat ended successfully</response>
    /// <response code="400">Invalid request (e.g., already ended)</response>
    /// <response code="403">User is not a participant</response>
    /// <response code="404">Chat session not found</response>
    /// <response code="401">Unauthorized</response>
    [HttpPost("{sessionId}/end")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ChatControlResponseDto>> EndSession(Guid sessionId)
    {
        var userId = Guid.Parse(
            User.FindFirstValue(ClaimTypes.NameIdentifier)!
        );

        try
        {
            var result = await _moderationService.EndSessionAsync(sessionId, userId);
            return Ok(result);
        }
        catch (ArgumentException ex) when (ex.Message.Contains("not a participant"))
        {
            return Forbid();
        }
        catch (ArgumentException ex) when (ex.Message.Contains("not found"))
        {
            return NotFound(new { error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Retrieves paginated message history for a chat session.
    /// Only participants can view message history.
    /// </summary>
    /// <remarks>
    /// Messages are returned in chronological order (oldest first).
    /// Anonymity settings are respected - messages from users with IsIdentityRevealed=false
    /// will be marked as anonymous.
    /// </remarks>
    /// <param name="sessionId">The chat session ID</param>
    /// <param name="page">Page number (1-based), defaults to 1</param>
    /// <param name="pageSize">Number of messages per page (max 500), defaults to 50</param>
    /// <returns>Paginated list of messages</returns>
    /// <response code="200">Message history retrieved successfully</response>
    /// <response code="400">Invalid pagination parameters</response>
    /// <response code="403">User is not a participant</response>
    /// <response code="404">Chat session not found</response>
    /// <response code="401">Unauthorized</response>
    [HttpGet("{sessionId}/messages")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<List<ChatMessageDto>>> GetMessages(
        Guid sessionId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        // Validate pagination parameters
        if (page < 1 || pageSize < 1 || pageSize > 500)
            return BadRequest(new { error = "Page must be >= 1, pageSize must be between 1 and 500." });

        var userId = Guid.Parse(
            User.FindFirstValue(ClaimTypes.NameIdentifier)!
        );

        try
        {
            var messages = await _moderationService.GetSessionMessagesAsync(
                sessionId,
                userId,
                page,
                pageSize
            );

            return Ok(messages);
        }
        catch (ArgumentException ex) when (ex.Message.Contains("not a participant"))
        {
            return Forbid();
        }
        catch (ArgumentException ex) when (ex.Message.Contains("not found"))
        {
            return NotFound(new { error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
    
    [HttpPost("peer")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> CreatePeerSession([FromBody] CreatePeerChatRequest request)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var sessionId = await _service.CreatePeerSessionAsync(userId, request.TargetUserId);
        return Ok(new { sessionId });
    }

    /// <summary>
    /// Retrieves a list of all active chat sessions for the current user.
    /// </summary>
    /// <returns>A list of active chat sessions.</returns>
    /// <response code="200">Returns the list of active sessions.</response>
    /// <response code="401">Unauthorized.</response>
    [HttpGet("active")]
    [ProducesResponseType(typeof(List<ChatSessionListDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetActiveSessions()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var sessions = await _service.GetActiveSessions(userId);
        return Ok(sessions);
    }

    /// <summary>
    /// Retrieves detailed information about a specific chat session.
    /// </summary>
    /// <param name="sessionId">The ID of the chat session.</param>
    /// <returns>Detailed information about the chat session.</returns>
    /// <response code="200">Returns the session details.</response>
    /// <response code="401">Unauthorized.</response>
    /// <response code="403">User is not a participant of the session.</response>
    /// <response code="404">Session not found.</response>
    [HttpGet("{sessionId}/detail")]
    [ProducesResponseType(typeof(ChatSessionDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSessionDetail(Guid sessionId)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        try
        {
            var sessionDetail = await _service.GetSessionDetail(sessionId, userId);
            return Ok(sessionDetail);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (Exception ex) when (ex.Message.Contains("not found"))
        {
            return NotFound(new { error = ex.Message });
        }
    }
}
