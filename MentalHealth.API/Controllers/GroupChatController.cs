using MentalHealth.Service.Interfaces;
using MentalHealth.Shared.DTOs.GroupChat;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MentalHealth.API.Controllers;

[ApiController]
[Route("api/groupChat")]
[Authorize]
public class GroupChatController : ControllerBase
{
    private readonly IGroupChatService _groupChatService;

    public GroupChatController(IGroupChatService groupChatService)
    {
        _groupChatService = groupChatService;
    }

    /// <summary>
    /// Gets all active group chat rooms available to join
    /// </summary>
    [HttpGet("active")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<GroupRoomDto>>> GetActiveGroupChats()
    {
        var rooms = await _groupChatService.GetAllActiveGroupChatsAsync();
        return Ok(rooms);
    }

    /// <summary>
    /// Joins a group chat room
    /// </summary>
    [HttpPost("join")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<JoinGroupResultDto>> JoinGroupChat([FromBody] JoinGroupRequestDto dto)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        try
        {
            var result = await _groupChatService.JoinGroupChatAsync(userId, dto);
            
            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Leaves a group chat room
    /// </summary>
    [HttpPost("leave")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> LeaveGroupChat([FromBody] JoinGroupRequestDto dto)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        try
        {
            await _groupChatService.LeaveGroupChatAsync(userId, dto.ChatSessionId);
            return Ok(new { message = "Successfully left group chat" });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Gets message history for a group chat with pagination
    /// </summary>
    [HttpGet("messages/{chatSessionId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<GroupMessageDto>>> GetGroupMessages(
        Guid chatSessionId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        try
        {
            var messages = await _groupChatService.GetGroupMessagesAsync(chatSessionId, page, pageSize);
            return Ok(messages);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}