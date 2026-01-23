using MentalHealth.Service.Interfaces;
using MentalHealth.Shared.DTOs.ExpertInvitation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MentalHealth.API.Controllers;

[ApiController]
[Route("api/expert/invitations")]
[Authorize(Roles = "Expert")]
public class ExpertInvitationsController : ControllerBase
{
    private readonly IExpertInvitationService _service;

    public ExpertInvitationsController(IExpertInvitationService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<IActionResult> Send(ExpertInviteCreateDto dto)
    {
        var expertId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _service.SendInviteAsync(expertId, dto);
        return Ok();
    }

    [HttpGet("low-mood-users")]
public async Task<IActionResult> LowMoodUsers()
{
    return Ok(await _service.GetLowMoodUsersAsync());
}

}
