using System.Security.Claims;
using Core.Entities.Auth;

namespace Core.Interfaces.Auth
{
    public interface IJwtService
    {
        string GenerateAccessToken(User user);
        string GenerateRefreshToken();
        ClaimsPrincipal? GetClaimsFromToken(string token);
        bool ValidateToken(string token);
        DateTime GetTokenExpiration(string token);
    }
}