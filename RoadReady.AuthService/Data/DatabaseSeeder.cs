using Microsoft.EntityFrameworkCore;
using RoadReady.AuthService.Models;
using RoadReady.Shared.Enums;

namespace RoadReady.AuthService.Data;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AuthDbContext>();

        await context.Database.MigrateAsync();

        var adminUser = await context.Users.FirstOrDefaultAsync(u => u.Email == "admin@roadready.com");

        if (adminUser == null)
        {
            //create admin
            adminUser = new User
            {
                FirstName = "System",
                LastName = "Admin",
                Email = "admin@roadready.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("StrongPass@123!"), 
                PhoneNumber = "0000000000",
                Role = UserRole.Admin,
                CreatedAt = DateTime.UtcNow
            };
            await context.Users.AddAsync(adminUser);
        }
        else
        {
            adminUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword("StrongPass@123!");
            adminUser.Role = UserRole.Admin;
            context.Users.Update(adminUser);
        }

        await context.SaveChangesAsync();
    }
}