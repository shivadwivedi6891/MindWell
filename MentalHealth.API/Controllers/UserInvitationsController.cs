using MentalHealth.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using MentalHealth.Shared.DTOs.Chat;



namespace MentalHealth.API.Controllers;

[ApiController]
[Route("api/user/invitations")]
[Authorize]
public class UserInvitationsController : ControllerBase
{
    private readonly IExpertInvitationService _service;

    public UserInvitationsController(IExpertInvitationService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return Ok(await _service.GetUserInvitesAsync(userId));
    }

    [HttpPost("{id}/accept")]
    public async Task<IActionResult> Accept(Guid id)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _service.AcceptAsync(id, userId);
        return Ok();
    }

    [HttpPost("{id}/decline")]
    public async Task<IActionResult> Decline(Guid id)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _service.DeclineAsync(id, userId);
        return Ok();
    }

    [HttpPost("{id}/accept-chat")]
public async Task<IActionResult> AcceptAndCreateChat(Guid id)
{
    var userId = Guid.Parse(
        User.FindFirstValue(ClaimTypes.NameIdentifier)!
    );

    var result = await _service.AcceptAndCreateChatAsync(id, userId);
    return Ok(result);
}



}
