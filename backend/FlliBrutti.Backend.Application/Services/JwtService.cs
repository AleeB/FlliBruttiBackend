using FlliBrutti.Backend.Application.IContext;
using FlliBrutti.Backend.Application.IServices;
using FlliBrutti.Backend.Application.Models;
using FlliBrutti.Backend.Application.Responses;
using FlliBrutti.Backend.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace FlliBrutti.Backend.Application.Services
{
    public class JwtService : IJwtService
    {
        private readonly IFlliBruttiContext _context;
        private readonly ILogger<JwtService> _logger;
        private readonly IConfiguration _configuration;
        private readonly string _secretKey;
        private readonly string _issuer;
        private readonly string _audience;
        private readonly int _accessTokenExpirationHours;
        private readonly int _refreshTokenExpirationDays;

        public JwtService(
            IFlliBruttiContext context,
            ILogger<JwtService> logger,
            IConfiguration configuration)
        {
            _context = context;
            _logger = logger;
            _configuration = configuration;

            _secretKey = _configuration["Jwt:SecretKey"]
                ?? throw new InvalidOperationException("JWT SecretKey not configured");
            _issuer = _configuration["Jwt:Issuer"]
                ?? throw new InvalidOperationException("JWT Issuer not configured");
            _audience = _configuration["Jwt:Audience"]
                ?? throw new InvalidOperationException("JWT Audience not configured");
            _accessTokenExpirationHours = int.Parse(_configuration["Jwt:AccessTokenExpirationHours"] ?? "2");
            _refreshTokenExpirationDays = int.Parse(_configuration["Jwt:RefreshTokenExpirationDays"] ?? "15");
        }

        public async Task<JwtTokenResponse> GenerateTokensAsync(User user, string ipAddress)
        {
            try
            {
                var accessTokenExpiration = DateTime.UtcNow.AddHours(_accessTokenExpirationHours);
                var refreshTokenExpiration = DateTime.UtcNow.AddDays(_refreshTokenExpirationDays);

                // Genera Access Token
                var accessToken = GenerateAccessToken(user, accessTokenExpiration);

                // Genera Refresh Token
                var refreshToken = GenerateRefreshToken();

                // Salva il Refresh Token nel database
                var refreshTokenEntity = new RefreshToken
                {
                    Token = refreshToken,
                    UserId = user.IdPerson,
                    ExpiresAt = refreshTokenExpiration,
                    CreatedAt = DateTime.UtcNow,
                    IsRevoked = false
                };

                await _context.RefreshTokens.AddAsync(refreshTokenEntity);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Generated tokens for user {user.Email} from IP {ipAddress}");

                return new JwtTokenResponse
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    AccessTokenExpiration = accessTokenExpiration,
                    RefreshTokenExpiration = refreshTokenExpiration
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error generating tokens for user {user.Email}");
                throw;
            }
        }

        public async Task<JwtTokenResponse?> RefreshTokenAsync(string refreshToken, string ipAddress)
        {
            try
            {
                var storedToken = await _context.RefreshTokens
                    .Include(rt => rt.User)
                        .ThenInclude(u => u.IdPersonNavigation)
                    .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

                if (storedToken == null || !storedToken.IsActive)
                {
                    _logger.LogWarning($"Invalid or expired refresh token attempted from IP {ipAddress}");
                    return null;
                }

                // Revoca il vecchio token
                storedToken.IsRevoked = true;
                storedToken.RevokedAt = DateTime.UtcNow;
                storedToken.RevokedByIp = ipAddress;

                _context.RefreshTokens.Update(storedToken);
                await _context.SaveChangesAsync();

                // Genera nuovi token
                return await GenerateTokensAsync(storedToken.User, ipAddress);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing token");
                return null;
            }
        }

        public async Task<bool> RevokeTokenAsync(string refreshToken, string ipAddress)
        {
            try
            {
                var storedToken = await _context.RefreshTokens
                    .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

                if (storedToken == null || !storedToken.IsActive)
                {
                    _logger.LogWarning($"Attempted to revoke invalid token from IP {ipAddress}");
                    return false;
                }

                storedToken.IsRevoked = true;
                storedToken.RevokedAt = DateTime.UtcNow;
                storedToken.RevokedByIp = ipAddress;

                _context.RefreshTokens.Update(storedToken);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Token revoked from IP {ipAddress}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking token");
                return false;
            }
        }

        public ClaimsPrincipal? ValidateAccessToken(string accessToken)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_secretKey);

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _issuer,
                    ValidateAudience = true,
                    ValidAudience = _audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                var principal = tokenHandler.ValidateToken(accessToken, validationParameters, out _);
                return principal;
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Token validation failed: {ex.Message}");
                return null;
            }
        }

        public async Task CleanupExpiredTokensAsync()
        {
            try
            {
                var expiredTokens = await _context.RefreshTokens
                    .Where(rt => rt.ExpiresAt < DateTime.UtcNow || rt.IsRevoked)
                    .ToListAsync();

                if (expiredTokens.Any())
                {
                    _context.RefreshTokens.RemoveRange(expiredTokens);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation($"Cleaned up {expiredTokens.Count} expired tokens");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning up expired tokens");
            }
        }

        private string GenerateAccessToken(User user, DateTime expiration)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.IdPerson.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Type.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("PersonName", user.IdPersonNavigation?.Name ?? ""),
                new Claim("PersonSurname", user.IdPersonNavigation?.Surname ?? "")
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _issuer,
                audience: _audience,
                claims: claims,
                expires: expiration,
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string GenerateRefreshToken()
        {
            var randomBytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }
    }
}