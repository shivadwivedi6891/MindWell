using MentalHealth.Repository.Entities;
using MentalHealth.Repository.Interfaces;
using MentalHealth.Service.Interfaces;
using MentalHealth.Shared.DTOs.Admin;
using Microsoft.AspNetCore.Identity;

namespace MentalHealth.Service.Implementations;

public class AdminUserService : IAdminUserService
{
    private readonly IAdminUserRepository _userRepository;
    private readonly IExpertProfileRepository _expertProfileRepository;
    private readonly PasswordHasher<User> _passwordHasher;

    public AdminUserService(IAdminUserRepository userRepository, IExpertProfileRepository expertProfileRepository)
    {
        _userRepository = userRepository;
        _expertProfileRepository = expertProfileRepository;
        _passwordHasher = new PasswordHasher<User>();
    }

    public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
    {
        var users = await _userRepository.GetAllUsersAsync();
        return users.Select(u => new UserDto
        {
            Id = u.Id,
            Email = u.Email,
            DisplayName = u.DisplayName,
            Role = u.Role,
            IsActive = u.IsActive,
            IsAnonymous = u.IsAnonymous,
            CreatedAt = u.CreatedAt
        });
    }

    public async Task<UserDto?> GetUserByIdAsync(Guid id)
    {
        var user = await _userRepository.GetUserByIdAsync(id);
        if (user == null) return null;
        return new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            DisplayName = user.DisplayName,
            Role = user.Role,
            IsActive = user.IsActive,
            IsAnonymous = user.IsAnonymous,
            CreatedAt = user.CreatedAt
        };
    }

    public async Task<UserDto> CreateUserAsync(CreateUserDto createUserDto)
    {
        var existingUser = await _userRepository.GetUserByEmailAsync(createUserDto.Email);
        if (existingUser != null)
        {
            throw new ArgumentException("User with this email already exists.");
        }

        var user = new User
        {
            Email = createUserDto.Email,
            DisplayName = createUserDto.DisplayName,
            Role = createUserDto.Role,
            IsActive = createUserDto.IsActive,
            IsAnonymous = createUserDto.IsAnonymous
        };
        user.PasswordHash = _passwordHasher.HashPassword(user, createUserDto.Password);

        if (user.Role == "Expert")
        {
            user.ExpertProfile = new ExpertProfile { IsApproved = true };
        }

        var createdUser = await _userRepository.CreateUserAsync(user);
        return new UserDto 
        {
            Id = createdUser.Id,
            Email = createdUser.Email,
            DisplayName = createdUser.DisplayName,
            Role = createdUser.Role,
            IsActive = createdUser.IsActive,
            IsAnonymous = createdUser.IsAnonymous,
            CreatedAt = createdUser.CreatedAt
        };
    }

    public async Task<bool> UpdateUserAsync(Guid id, UpdateUserDto updateUserDto)
    {
        var user = await _userRepository.GetUserByIdAsync(id);
        if (user == null) return false;

        user.Email = updateUserDto.Email;
        user.DisplayName = updateUserDto.DisplayName;
        user.Role = updateUserDto.Role;
        user.IsActive = updateUserDto.IsActive;
        user.IsAnonymous = updateUserDto.IsAnonymous;
        
        var isCurrentlyExpert = user.ExpertProfile != null;
        var isTargetExpert = user.Role == "Expert";

        if (isTargetExpert && !isCurrentlyExpert)
        {
            user.ExpertProfile = new ExpertProfile { IsApproved = true };
        }
        else if (!isTargetExpert && isCurrentlyExpert)
        {
            await _expertProfileRepository.DeleteAsync(user.ExpertProfile!);
        }

        await _userRepository.UpdateUserAsync(user);
        return true;
    }

    public async Task<bool> DeleteUserAsync(Guid id)
    {
        var user = await _userRepository.GetUserByIdAsync(id);
        if (user == null) return false;

        await _userRepository.DeleteUserAsync(user);
        return true;
    }
}
