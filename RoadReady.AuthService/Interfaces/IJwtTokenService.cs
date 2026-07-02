using RoadReady.AuthService.Models;

namespace RoadReady.AuthService.Interfaces;

public interface IJwtTokenService
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
}
