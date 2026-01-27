using MentalHealth.Repository.Entities;

namespace MentalHealth.Repository.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email);
    Task AddAsync(User user);
    Task SaveChangesAsync();

    Task SetReadyToChatAsync(Guid userId, bool ready);

 



    Task<User?> GetByIdAsync(Guid userId);

Task<List<User>> GetReadyUsersAsync(Guid currentUserId);





Task UpdateAsync(User user);


}
