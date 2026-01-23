using MentalHealth.Repository.Entities;

namespace MentalHealth.Repository.Interfaces;

public interface IAdminUserRepository
{
    Task<IEnumerable<User>> GetAllUsersAsync();
    Task<User?> GetUserByIdAsync(Guid id);
    Task<User> CreateUserAsync(User user);
    Task UpdateUserAsync(User user);
    Task DeleteUserAsync(User user);
    Task<User?> GetUserByEmailAsync(string email);
}
