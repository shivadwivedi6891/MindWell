using MentalHealth.Service.Interfaces;
using MentalHealth.Shared.DTOs.Chat;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MentalHealth.API.Controllers;

/// <summary>
/// Handles peer-to-peer (user-to-user) chat operations
/// </summary>
[ApiController]
[Route("api/chat/peer")]
[Authorize]
public class PeerChatController : ControllerBase
{
    private readonly IPeerChatService _peerChatService;

    public PeerChatController(IPeerChatService peerChatService)
    {
        _peerChatService = peerChatService;
    }

    /// <summary>
    /// Creates or retrieves an existing peer chat session between two users.
    /// </summary>
    /// <remarks>
    /// If a peer session already exists between the two users, it will be reused.
    /// Otherwise, a new session is created.
    /// </remarks>
    /// <param name="request">Contains the target user ID</param>
    /// <returns>Session ID and whether it was newly created</returns>
    /// <response code="200">Session created or retrieved successfully</response>
    /// <response code="400">Invalid request (e.g., target user doesn't exist)</response>
    /// <response code="401">Unauthorized</response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<CreatePeerChatResponse>> CreateOrGetPeerChat(
        [FromBody] CreatePeerChatRequest request)
    {
        // Extract current user ID from JWT claims
        var currentUserId = Guid.Parse(
            User.FindFirstValue(ClaimTypes.NameIdentifier)!
        );

        try
        {
            var (sessionId, isNew) = await _peerChatService.CreateOrGetPeerSessionAsync(
                currentUserId,
                request.TargetUserId
            );

            return Ok(new CreatePeerChatResponse
            {
                SessionId = sessionId,
                IsNew = isNew
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Retrieves peer chat session metadata and participant information.
    /// </summary>
    /// <remarks>
    /// Only the current user (a participant in the session) can view session details.
    /// Identity information respects the IsIdentityRevealed flag.
    /// </remarks>
    /// <param name="sessionId">The peer chat session ID</param>
    /// <returns>Session metadata including participant information</returns>
    /// <response code="200">Session details retrieved successfully</response>
    /// <response code="403">User is not a participant in this session</response>
    /// <response code="404">Session not found or is not a peer chat</response>
    /// <response code="401">Unauthorized</response>
    [HttpGet("{sessionId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PeerChatSessionDto>> GetPeerSession(Guid sessionId)
    {
        var currentUserId = Guid.Parse(
            User.FindFirstValue(ClaimTypes.NameIdentifier)!
        );

        var session = await _peerChatService.GetPeerSessionAsync(sessionId, currentUserId);

        if (session == null)
            return NotFound(new { error = "Peer chat session not found or access denied." });

        return Ok(session);
    }

    /// <summary>
    /// Retrieves paginated message history for a peer chat session.
    /// </summary>
    /// <remarks>
    /// Messages are returned in chronological order (oldest first).
    /// Only the current user (a participant) can view message history.
    /// Messages respect the anonymity settings of senders.
    /// </remarks>
    /// <param name="sessionId">The peer chat session ID</param>
    /// <param name="page">Page number (1-based), defaults to 1</param>
    /// <param name="pageSize">Number of messages per page, defaults to 50</param>
    /// <returns>Paginated list of messages</returns>
    /// <response code="200">Message history retrieved successfully</response>
    /// <response code="400">Invalid pagination parameters</response>
    /// <response code="403">User is not a participant in this session</response>
    /// <response code="404">Session not found or is not a peer chat</response>
    /// <response code="401">Unauthorized</response>
    [HttpGet("{sessionId}/messages")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<List<ChatMessageDto>>> GetMessageHistory(
        Guid sessionId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        // Validate pagination parameters
        if (page < 1 || pageSize < 1 || pageSize > 500)
            return BadRequest(new { error = "Page must be >= 1, pageSize must be between 1 and 500." });

        var currentUserId = Guid.Parse(
            User.FindFirstValue(ClaimTypes.NameIdentifier)!
        );

        try
        {
            var messages = await _peerChatService.GetMessageHistoryAsync(
                sessionId,
                currentUserId,
                page,
                pageSize
            );

            return Ok(messages);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }
}
