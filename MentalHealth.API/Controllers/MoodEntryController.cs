using MentalHealth.Service.Interfaces;
using MentalHealth.Shared.DTOs.Mood;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MentalHealth.API.Controllers;

[ApiController]
[Route("api/mood/entry")]
[Authorize]
public class MoodEntryController : ControllerBase
{
    private readonly IMoodEntryService _service;

    public MoodEntryController(IMoodEntryService service)
    {
        _service = service;
    }

   [HttpPost]
public async Task<IActionResult> Submit([FromBody] MoodEntryCreateDto dto)
{
    var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    var result = await _service.SubmitAsync(userId, dto);
    return Ok(result);
}


    [HttpGet("today")]
    public async Task<IActionResult> Today()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _service.GetTodayAsync(userId);
        return Ok(result);
    }
}
