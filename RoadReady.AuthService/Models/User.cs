using RoadReady.Shared.Enums;

namespace RoadReady.AuthService.Models;

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    
    public string? PasswordHash { get; set; } 
    
    public string PhoneNumber { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.Customer;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public string? GoogleId { get; set; }
    public string? AuthProvider { get; set; }
    public string? ProfileImageUrl { get; set; }
}