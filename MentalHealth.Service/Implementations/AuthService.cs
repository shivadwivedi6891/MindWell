using MentalHealth.Repository.Entities;
using MentalHealth.Repository.Interfaces;
using MentalHealth.Service.Interfaces;
using MentalHealth.Shared.DTOs.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MentalHealth.Service.Implementations;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IConfiguration _configuration;
    private readonly PasswordHasher<User> _passwordHasher;

    public AuthService(IUserRepository userRepository, IConfiguration configuration)
    {
        _userRepository = userRepository;
        _configuration = configuration;
        _passwordHasher = new PasswordHasher<User>();
    }

    // ---------------- REGISTER ----------------
    public async Task RegisterAsync(RegisterRequestDto request)
    {
        var existingUser = await _userRepository.GetByEmailAsync(request.Email);
        if (existingUser != null)
            throw new InvalidOperationException("Email already registered.");

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            DisplayName = request.DisplayName,
            Role = "User",
            IsAnonymous = false,
            IsActive = true
        };

        user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

        await _userRepository.AddAsync(user);
        await _userRepository.SaveChangesAsync();
    }

    // ---------------- LOGIN ----------------
    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);
        if (user == null)
            throw new UnauthorizedAccessException("Invalid credentials");

        var result = _passwordHasher.VerifyHashedPassword(
            user,
            user.PasswordHash,
            request.Password
        );

        if (result == PasswordVerificationResult.Failed)
            throw new UnauthorizedAccessException("Invalid credentials");

        var token = GenerateJwtToken(user);
         return new LoginResponseDto
   
    {
        Token = token,
        Role = user.Role,                
        IsAnonymous = user.IsAnonymous,
        UserId = user.Id,
        Email = user.Email
    };


    }

    // ---------------- TOGGLE ANONYMOUS ----------------
    public async Task<LoginResponseDto> ToggleAnonymousAsync(Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            throw new KeyNotFoundException("User not found");

        user.IsAnonymous = !user.IsAnonymous;

        await _userRepository.UpdateAsync(user);
        await _userRepository.SaveChangesAsync();

        var token = GenerateJwtToken(user);
         return new LoginResponseDto
    {
        Token = token,
        Role = user.Role,
        IsAnonymous = user.IsAnonymous,
        UserId = user.Id,
        Email = user.Email
    };
    }

    // ---------------- JWT GENERATOR ----------------
   private string GenerateJwtToken(User user)
{
    var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.Email, user.Email),
        new Claim(ClaimTypes.Role, user.Role),
        new Claim("isAnonymous", user.IsAnonymous.ToString())
    };

    var jwt = _configuration.GetSection("Jwt");

    var key = new SymmetricSecurityKey(
        Encoding.UTF8.GetBytes(jwt["Key"]!)
    );

    var token = new JwtSecurityToken(
        issuer: jwt["Issuer"],
        audience: jwt["Audience"],
        claims: claims,
        expires: DateTime.UtcNow.AddMinutes(
            double.Parse(jwt["ExpiryMinutes"]!)
        ),
        signingCredentials: new SigningCredentials(
            key,
            SecurityAlgorithms.HmacSha256
        )
    );

    return new JwtSecurityTokenHandler().WriteToken(token);
}

}
