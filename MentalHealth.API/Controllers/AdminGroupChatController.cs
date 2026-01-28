using MentalHealth.Service.Interfaces;
using MentalHealth.Shared.DTOs.GroupChat;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MentalHealth.API.Controllers;

[ApiController]
[Route("api/admin/groupChat")]
[Authorize(Roles = "Admin")]
public class AdminGroupChatController : ControllerBase
{
    private readonly IGroupChatService _groupChatService;

    public AdminGroupChatController(IGroupChatService groupChatService)
    {
        _groupChatService = groupChatService;
    }

    /// <summary>
    /// Creates a new group chat topic (Admin only)
    /// </summary>
    [HttpPost("topics/create")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Guid>> CreateGroupTopic([FromBody] CreateGroupTopicDto dto)
    {
        try
        {
            var topicId = await _groupChatService.CreateGroupTopicAsync(dto);
            return CreatedAtAction(nameof(CreateGroupTopic), new { id = topicId }, topicId);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Opens a new group chat session for a topic (Admin only)
    /// </summary>
    [HttpPost("sessions/open")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Guid>> OpenGroupSession([FromBody] OpenGroupSessionDto dto)
    {
        try
        {
            var sessionId = await _groupChatService.OpenGroupSessionAsync(dto);
            return CreatedAtAction(nameof(OpenGroupSession), new { id = sessionId }, sessionId);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Sets a group session active or inactive (Admin only)
    /// </summary>
    [HttpPost("sessions/{sessionId}/setActive")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetGroupSessionActive(
        Guid sessionId,
        [FromBody] bool isActive)
    {
        try
        {
            await _groupChatService.SetGroupSessionActiveAsync(sessionId, isActive);
            return Ok(new { message = "Session status updated" });
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Gets all active group chat sessions (Admin only)
    /// </summary>
    [HttpGet("sessions/active")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<GroupRoomDto>>> GetAllActiveGroupChats()
    {
        var rooms = await _groupChatService.GetAllActiveGroupChatsAsync();
        return Ok(rooms);
    }

    /// <summary>
    /// Gets all group chat sessions including inactive ones (Admin only)
    /// </summary>
    [HttpGet("sessions/all")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<GroupRoomDto>>> GetAllGroupChats()
    {
        var rooms = await _groupChatService.GetAllGroupChatsAsync();
        return Ok(rooms);
    }
}