using MentalHealth.Repository.Entities;
using MentalHealth.Repository.Interfaces;
using MentalHealth.Service.Interfaces;
using MentalHealth.Shared.DTOs.Auth;
using Microsoft.AspNetCore.Identity;

namespace MentalHealth.Service.Implementations;

public class ExpertService : IExpertService
{
    private readonly IUserRepository _userRepository;
    private readonly IExpertProfileRepository _expertRepo;
    private readonly PasswordHasher<User> _passwordHasher = new();

    public ExpertService(
        IUserRepository userRepository,
        IExpertProfileRepository expertRepo)
    {
        _userRepository = userRepository;
        _expertRepo = expertRepo;
    }

    public async Task RegisterExpertAsync(ExpertRegisterDto dto)
    {
        var existing = await _userRepository.GetByEmailAsync(dto.Email);
        if (existing != null)
            throw new Exception("Email already registered");

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = dto.Email,
            DisplayName = dto.DisplayName,
            Role = "Expert",
            IsActive = true
        };

        user.PasswordHash = _passwordHasher.HashPassword(user, dto.Password);

        await _userRepository.AddAsync(user);

        var profile = new ExpertProfile
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Qualification = dto.Qualification,
            ExperienceYears = dto.ExperienceYears,
            Specialization = dto.Specialization,
            IsApproved = false
        };

        await _expertRepo.AddAsync(profile);

        await _userRepository.SaveChangesAsync();
        await _expertRepo.SaveChangesAsync();
    }

    public async Task<List<ExpertApprovalResponseDto>> GetPendingExpertsAsync()
    {
        var experts = await _expertRepo.GetPendingAsync();

        return experts.Select(e => new ExpertApprovalResponseDto
        {
            ExpertProfileId = e.Id,
            ExpertName = e.User.DisplayName,
            Qualification = e.Qualification,
            ExperienceYears = e.ExperienceYears,
            Specialization = e.Specialization,
            IsApproved = e.IsApproved
        }).ToList();
    }

    public async Task ApproveExpertAsync(Guid expertProfileId, Guid adminId)
    {
        var profile = await _expertRepo.GetByIdAsync(expertProfileId);
        if (profile == null)
            throw new Exception("Expert profile not found");

        profile.IsApproved = true;
        profile.ApprovedAt = DateTime.UtcNow;
        profile.ApprovedByAdminId = adminId;

        await _expertRepo.SaveChangesAsync();
    }

    public async Task RejectExpertAsync(Guid expertProfileId)
    {
        var profile = await _expertRepo.GetByIdAsync(expertProfileId);
        if (profile == null)
            throw new Exception("Expert profile not found");

        profile.IsApproved = false;
        await _expertRepo.SaveChangesAsync();
    }
}
