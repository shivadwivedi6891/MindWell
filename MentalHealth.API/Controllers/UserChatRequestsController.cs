using System.Security.Claims;
using MentalHealth.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MentalHealth.Shared.DTOs.Chat;



namespace MentalHealth.API.Controllers;

[Authorize]
[ApiController]
[Route("api/chat/requests")]
public class UserChatRequestsController : ControllerBase
{
    private readonly IUserChatRequestService _service;

    public UserChatRequestsController(IUserChatRequestService service)
    {
        _service = service;
    }

  
    [HttpPost]
    public async Task<IActionResult> Send([FromBody] SendChatRequestDto dto)
    {
        var userId = Guid.Parse(
            User.FindFirst(ClaimTypes.NameIdentifier)!.Value
        );

        await _service.SendRequestAsync(userId, dto.ToUserId);

        return Ok(new { message = "Request sent" });
    }

    //   [HttpGet("ready")]
    // public async Task<IActionResult> ReadyUsers()
    // {
    //     var userId = Guid.Parse(
    //         User.FindFirst(ClaimTypes.NameIdentifier)!.Value
    //     );

    //     var users = await _service.GetReadyUsersAsync(userId);

    //     return Ok(users);
    // }

    // [HttpPost]
    // public async Task<IActionResult> Set([FromBody] SetChatAvailabilityDto dto)
    // {
    //     var userId = Guid.Parse(
    //         User.FindFirst(ClaimTypes.NameIdentifier)!.Value
    //     );

    //     await _userService.SetChatAvailabilityAsync(userId, dto.ReadyToChat);

    //     return Ok(new { readyToChat = dto.ReadyToChat });
    // }

    // 2️⃣ Incoming requests
    [HttpGet("incoming")]
    public async Task<IActionResult> Incoming()
    {
        var userId = Guid.Parse(
            User.FindFirst(ClaimTypes.NameIdentifier)!.Value
        );

        var list = await _service.GetIncomingAsync(userId);

        return Ok(list);
    }

    // 3️⃣ Accept → create session
    [HttpPost("{id}/accept")]
    public async Task<IActionResult> Accept(Guid id)
    {
        var userId = Guid.Parse(
            User.FindFirst(ClaimTypes.NameIdentifier)!.Value
        );

        var result = await _service.AcceptAsync(id, userId);

        return Ok(result);  
    }

    // 4️⃣ Reject
    [HttpPost("{id}/reject")]
    public async Task<IActionResult> Reject(Guid id)
    {
        var userId = Guid.Parse(
            User.FindFirst(ClaimTypes.NameIdentifier)!.Value
        );

        await _service.RejectAsync(id, userId);

        return Ok(new { message = "Rejected" });
    }
}
