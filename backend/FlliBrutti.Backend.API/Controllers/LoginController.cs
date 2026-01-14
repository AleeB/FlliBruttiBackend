using FlliBrutti.Backend.Application.IServices;
using FlliBrutti.Backend.Application.Models;
using FlliBrutti.Backend.Application.Responses;
using FlliBrutti.Backend.Core.Enums;
using FlliBrutti.Backend.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Razor.TagHelpers;

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

                var isValid = await _loginService.LoginAsync(login);
                if (!isValid)
                {
                    _logger.LogWarning($"Failed login attempt for email: {login.Email}");
                    return Unauthorized(new { error = "Email or Password does not match or does not exist" });
                }

                var user = await _userService.GetUserByEmailAsync(login.Email);
                if (user == null)
                {
                    _logger.LogError($"User not found after successful login validation: {login.Email}");
                    return StatusCode(500, new { error = "Internal server error" });
                }

                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
                var u = new User
                {
                    IdPerson = user.IdPerson,
                    Email = user.Email,
                    Type = ((int)user.Type)
                };
                var tokens = await _jwtService.GenerateTokensAsync(u , ipAddress);

                _logger.LogInformation($"User {login.Email} logged in successfully from IP {ipAddress}");

                return Ok(new
                {
                    user = new
                    {
                        user.IdPerson,
                        user.Email,
                        user.Type,
                        Name = user.Name,
                        Surname = user.Surname,
                        DOB = user.DOB
                    },
                    tokens.AccessToken,
                    tokens.RefreshToken,
                    tokens.AccessTokenExpiration,
                    tokens.RefreshTokenExpiration
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error during login for email: {login?.Email}");
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
                    _logger.LogWarning($"Invalid refresh token attempt from IP {ipAddress}");
                    return Unauthorized(new { error = "Invalid or expired refresh token" });
                }

                _logger.LogInformation($"Token refreshed successfully from IP {ipAddress}");

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

        [HttpPost("revoke")]
        [Authorize]
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
                    _logger.LogWarning($"Failed to revoke token from IP {ipAddress}");
                    return BadRequest(new { error = "Failed to revoke token" });
                }

                _logger.LogInformation($"Token revoked successfully from IP {ipAddress}");
                return Ok(new { message = "Token revoked successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during token revocation");
                return StatusCode(500, new { error = "Internal server error", details = ex.Message });
            }
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout([FromBody] RefreshTokenRequest request)
        {
            // Logout è identico a revoke, ma semanticamente più chiaro
            return await RevokeToken(request);
        }

    }
}
