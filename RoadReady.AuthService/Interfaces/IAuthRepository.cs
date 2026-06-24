using RoadReady.AuthService.Models;

namespace RoadReady.AuthService.Interfaces;

public interface IAuthRepository
{
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByIdAsync(Guid id);
    Task<User?> GetByGoogleIdAsync(string googleId);
    Task<bool> EmailExistsAsync(string email);
    Task AddUserAsync(User user);
    Task UpdateUserAsync(User user);
    Task SaveChangesAsync();
}
