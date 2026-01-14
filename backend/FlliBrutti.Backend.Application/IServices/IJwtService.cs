using FlliBrutti.Backend.Application.Models;
using FlliBrutti.Backend.Application.Responses;
using FlliBrutti.Backend.Core.Models;
using System.Security.Claims;

namespace FlliBrutti.Backend.Application.IServices
{
    public interface IJwtService
    {
        Task<JwtTokenResponse> GenerateTokensAsync(User user, string ipAddress);
        Task<JwtTokenResponse?> RefreshTokenAsync(string refreshToken, string ipAddress);
        Task<bool> RevokeTokenAsync(string refreshToken, string ipAddress);
        ClaimsPrincipal? ValidateAccessToken(string accessToken);
        Task CleanupExpiredTokensAsync();
    }
}
