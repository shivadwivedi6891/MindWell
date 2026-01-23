using MentalHealth.Service.Interfaces;
using MentalHealth.Shared.DTOs.Mood;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MentalHealth.API.Controllers;

[ApiController]
[Route("api/mood-questions")]
public class MoodQuestionsController : ControllerBase
{
    private readonly IMoodQuestionService _service;

    public MoodQuestionsController(IMoodQuestionService service)
    {
        _service = service;
    }

    // Admin only
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Create(MoodQuestionCreateDto dto)
    {
        await _service.CreateAsync(dto);
        return Ok();
    }

    // User-facing
    [Authorize]
    [HttpGet("active")]
    public async Task<IActionResult> GetActive()
    {
        return Ok(await _service.GetActiveAsync());
    }

    // Admin only
    [Authorize(Roles = "Admin")]
    [HttpPatch("{id}/toggle")]
    public async Task<IActionResult> Toggle(Guid id)
    {
        await _service.ToggleActiveAsync(id);
        return Ok();
    }
}
