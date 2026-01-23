using MentalHealth.Repository.Data;
using MentalHealth.Repository.Entities;
using MentalHealth.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MentalHealth.Repository.Implementations;

public class AdminUserRepository : IAdminUserRepository
{
    private readonly MentalHealthDbContext _context;

    public AdminUserRepository(MentalHealthDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<User>> GetAllUsersAsync()
    {
        return await _context.Users.Include(u => u.ExpertProfile).AsNoTracking().ToListAsync();
    }

    public async Task<User?> GetUserByIdAsync(Guid id)
    {
        return await _context.Users.Include(u => u.ExpertProfile).FirstOrDefaultAsync(u => u.Id == id);
    }
    
    public async Task<User?> GetUserByEmailAsync(string email)
    {
        return await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<User> CreateUserAsync(User user)
    {
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task UpdateUserAsync(User user)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteUserAsync(User user)
    {
        if (user.ExpertProfile != null)
        {
            _context.ExpertProfiles.Remove(user.ExpertProfile);
        }
        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
    }
}
