using MentalHealth.Service.Interfaces;
using MentalHealth.Shared.DTOs.Auth;
using Microsoft.AspNetCore.Mvc;

namespace MentalHealth.API.Controllers;

[ApiController]
[Route("api/expert")]
public class ExpertController : ControllerBase
{
    private readonly IExpertService _service;

    public ExpertController(IExpertService service)
    {
        _service = service;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(ExpertRegisterDto dto)
    {
        await _service.RegisterExpertAsync(dto);
        return Ok(new { message = "Expert registration submitted for approval" });
    }
}
