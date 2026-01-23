using MentalHealth.Repository.Entities;

namespace MentalHealth.Repository.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email);
    Task AddAsync(User user);
    Task SaveChangesAsync();

    Task<User?> GetByIdAsync(Guid userId);
Task UpdateAsync(User user);


}
