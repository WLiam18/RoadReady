using Microsoft.EntityFrameworkCore;
using RoadReady.AuthService.Data;
using RoadReady.AuthService.Interfaces;
using RoadReady.AuthService.Models;

namespace RoadReady.AuthService.Implementations;

public class AuthRepository : IAuthRepository
{
    private readonly AuthDbContext _context;

    public AuthRepository(AuthDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<User?> GetByIdAsync(Guid id)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<User?> GetByGoogleIdAsync(string googleId)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.GoogleId == googleId);
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        return await _context.Users.AnyAsync(u => u.Email == email);
    }

    public async Task AddUserAsync(User user)
    {
        await _context.Users.AddAsync(user);
    }

    public Task UpdateUserAsync(User user)
    {
        _context.Users.Update(user);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
