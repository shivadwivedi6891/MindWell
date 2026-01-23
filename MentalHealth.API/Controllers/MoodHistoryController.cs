using MentalHealth.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MentalHealth.API.Controllers;

[ApiController]
[Route("api/mood")]
[Authorize]
public class MoodHistoryController : ControllerBase
{
    private readonly IMoodTrendService _service;

    public MoodHistoryController(IMoodTrendService service)
    {
        _service = service;
    }

    [HttpGet("history")]
    public async Task<IActionResult> History()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return Ok(await _service.GetHistoryAsync(userId));
    }

    [HttpGet("trends")]
    public async Task<IActionResult> Trends()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return Ok(await _service.GetTrendsAsync(userId));
    }
}
