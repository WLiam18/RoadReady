using RoadReady.AuthService.Models;

namespace RoadReady.AuthService.Interfaces;

public interface IJwtTokenService
{
    string GenerateToken(User user);
}
