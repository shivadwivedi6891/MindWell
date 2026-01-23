namespace MentalHealth.Repository.Interfaces;
using MentalHealth.Repository.Entities;

public interface IExpertProfileRepository
{
    Task AddAsync(ExpertProfile profile);
     Task<ExpertProfile?> GetByUserIdAsync(Guid userId);

     Task<ExpertProfile?> GetByIdAsync(Guid id);
      Task<List<ExpertProfile>> GetPendingAsync();
      Task DeleteAsync(ExpertProfile profile);
       Task SaveChangesAsync();
}
