using MentalHealth.Repository.Data;
using MentalHealth.Repository.Entities;
using MentalHealth.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MentalHealth.Repository.Implementations;

public class UserRepository : IUserRepository
{
    private readonly MentalHealthDbContext _context;

    public UserRepository(MentalHealthDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task AddAsync(User user)
    {
        await _context.Users.AddAsync(user);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();


    }

public async Task GetByParticipantsAsync(Guid userId, bool ready)
{
    var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

    if (user == null)
        throw new Exception("User not found");

    user.ReadyToChat = ready;
    await _context.SaveChangesAsync();
}

//---------------

public async Task<List<User>> GetReadyUsersAsync(Guid currentUserId)
{
    return await _context.Users
        .Where(u => u.ReadyToChat == true && u.Id != currentUserId)
        .ToListAsync();
}


public async Task SetReadyToChatAsync(Guid userId, bool ready)
{
    var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

    if (user == null)
        throw new Exception("User not found");

    user.ReadyToChat = ready;
    await _context.SaveChangesAsync();
}
//------------------




    public async Task<User?> GetByIdAsync(Guid userId)
{
    return await _context.Users.FindAsync(userId);
}

public Task UpdateAsync(User user)
{
    _context.Users.Update(user);
    return Task.CompletedTask;
}

}
