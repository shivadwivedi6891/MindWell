using MentalHealth.Repository.Entities;


namespace MentalHealth.Repository.Interfaces;


public interface IUserChatRequestRepository
{
    Task AddAsync(UserChatRequest request);
    Task<UserChatRequest> GetByIdAsync(Guid id);

    Task<List<UserChatRequest>> GetIncomingAsync(Guid userId);

    Task<UserChatRequest> GetPendingBetweenAsync(Guid fromUserId, Guid toUserId);

    Task UpdateAsync(UserChatRequest request);
}
