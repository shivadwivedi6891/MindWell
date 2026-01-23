using MentalHealth.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MentalHealth.API.Controllers;

[ApiController]
[Route("api/admin/experts")]
[Authorize(Roles = "Admin")]
public class AdminExpertsController : ControllerBase
{
    private readonly IExpertService _service;

    public AdminExpertsController(IExpertService service)
    {
        _service = service;
    }

    [HttpGet("pending")]
    public async Task<IActionResult> Pending()
    {
        return Ok(await _service.GetPendingExpertsAsync());
    }

    [HttpPatch("{id}/approve")]
    public async Task<IActionResult> Approve(Guid id)
    {
        var adminId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _service.ApproveExpertAsync(id, adminId);
        return Ok();
    }

    [HttpPatch("{id}/reject")]
    public async Task<IActionResult> Reject(Guid id)
    {
        await _service.RejectExpertAsync(id);
        return Ok();
    }
}
