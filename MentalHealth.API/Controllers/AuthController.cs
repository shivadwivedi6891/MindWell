using MentalHealth.Service.Interfaces;
using MentalHealth.Shared.DTOs.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;




namespace MentalHealth.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
    {
        await _authService.RegisterAsync(request);
        return Ok(new { message = "User registered successfully." });
    }


    [HttpPost("login")]
public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
{
    var data = await _authService.LoginAsync(request);
    return Ok(new { data });
}


[Authorize]
[HttpGet("me")]
public IActionResult Me()
{
    return Ok(new
    {
        UserId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                 ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub),

        Email = User.FindFirstValue(ClaimTypes.Email),
        Role = User.FindFirstValue(ClaimTypes.Role),
        IsAnonymous = User.FindFirst("isAnonymous")?.Value
    });
}

[Authorize]
[HttpPost("anonymous")]
public async Task<IActionResult> ToggleAnonymous()
{
    var userId = Guid.Parse(
        User.FindFirstValue(ClaimTypes.NameIdentifier)!
    );

    var newData = await _authService.ToggleAnonymousAsync(userId);

    return Ok(new
    {
        token = newData
    });
}


}
