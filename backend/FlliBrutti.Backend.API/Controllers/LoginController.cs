using FlliBrutti.Backend.Application.IServices;
using FlliBrutti.Backend.Application.Models;
using FlliBrutti.Backend.Application.Responses;
using FlliBrutti.Backend.Core.Enums;
using FlliBrutti.Backend.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlliBrutti.Backend.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly ILogger<LoginController> _logger;
        private readonly ILoginService _loginService;
        private readonly IUserService _userService;
        private readonly IJwtService _jwtService;

        public LoginController(
            ILogger<LoginController> logger,
            ILoginService loginService,
            IUserService userService,
            IJwtService jwtService)
        {
            _logger = logger;
            _loginService = loginService;
            _userService = userService;
            _jwtService = jwtService;
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginDTO login)
        {
            try
            {
                if (login == null)
                {
                    _logger.LogWarning("Login attempt with null credentials");
                    return BadRequest(new { error = "Invalid login data" });
                }

                // 1️⃣ Valida le credenziali
                var isValid = await _loginService.LoginAsync(login);
                if (!isValid)
                {
                    _logger.LogWarning("Failed login attempt for email: {Email}", login.Email);
                    return Unauthorized(new { error = "Email or Password does not match or does not exist" });
                }

                // 2️⃣ Ottieni i dati utente (UNA SOLA VOLTA con AsNoTracking)
                var userResponse = await _userService.GetUserByEmailAsync(login.Email);
                if (userResponse == null)
                {
                    _logger.LogError("User not found after successful login validation: {Email}", login.Email);
                    return StatusCode(500, new { error = "Internal server error" });
                }

                // 3️⃣ Crea un oggetto User minimale per il JWT (SENZA caricare dal DB)
                var userForToken = new User
                {
                    IdPerson = userResponse.IdPerson,
                    Email = userResponse.Email,
                    Type = (int)userResponse.Type,
                    // Aggiungi Person inline per evitare un'altra query
                    IdPersonNavigation = new Person
                    {
                        IdPerson = userResponse.IdPerson,
                        Name = userResponse.Name,
                        Surname = userResponse.Surname,
                        DOB = userResponse.DOB
                    }
                };

                // 4️⃣ Genera i token
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
                var tokens = await _jwtService.GenerateTokensAsync(userForToken, ipAddress);

                _logger.LogInformation("User {Email} logged in successfully from IP {IpAddress}",
                    login.Email, ipAddress);

                return Ok(new
                {
                    user = new
                    {
                        userResponse.IdPerson,
                        userResponse.Email,
                        userResponse.Type,
                        userResponse.Name,
                        userResponse.Surname,
                        userResponse.DOB
                    },
                    tokens.AccessToken,
                    tokens.RefreshToken,
                    tokens.AccessTokenExpiration,
                    tokens.RefreshTokenExpiration
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for email: {Email}", login?.Email);
                return StatusCode(500, new { error = "Internal server error", details = ex.Message });
            }
        }

        [HttpPost("refresh")]
        [AllowAnonymous]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request?.RefreshToken))
                {
                    _logger.LogWarning("Refresh token attempt with empty token");
                    return BadRequest(new { error = "Refresh token is required" });
                }

                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
                var tokens = await _jwtService.RefreshTokenAsync(request.RefreshToken, ipAddress);

                if (tokens == null)
                {
                    _logger.LogWarning("Invalid refresh token attempt from IP {IpAddress}", ipAddress);
                    return Unauthorized(new { error = "Invalid or expired refresh token" });
                }

                _logger.LogInformation("Token refreshed successfully from IP {IpAddress}", ipAddress);

                return Ok(new
                {
                    tokens.AccessToken,
                    tokens.RefreshToken,
                    tokens.AccessTokenExpiration,
                    tokens.RefreshTokenExpiration
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during token refresh");
                return StatusCode(500, new { error = "Internal server error", details = ex.Message });
            }
        }

        [Authorize]
        [HttpPost("revoke")]
        public async Task<IActionResult> RevokeToken([FromBody] RefreshTokenRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request?.RefreshToken))
                {
                    _logger.LogWarning("Revoke token attempt with empty token");
                    return BadRequest(new { error = "Refresh token is required" });
                }

                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
                var result = await _jwtService.RevokeTokenAsync(request.RefreshToken, ipAddress);

                if (!result)
                {
                    _logger.LogWarning("Failed to revoke token from IP {IpAddress}", ipAddress);
                    return BadRequest(new { error = "Failed to revoke token" });
                }

                _logger.LogInformation("Token revoked successfully from IP {IpAddress}", ipAddress);
                return Ok(new { message = "Token revoked successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during token revocation");
                return StatusCode(500, new { error = "Internal server error", details = ex.Message });
            }
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] RefreshTokenRequest request)
        {
            return await RevokeToken(request);
        }
    }
}