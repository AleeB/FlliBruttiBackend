using FlliBrutti.Backend.Application.IServices;
using FlliBrutti.Backend.Application.Models;
using FlliBrutti.Backend.Application.Responses;
using FlliBrutti.Backend.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace FlliBrutti.Backend.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly ILogger<LoginController> _logger;
        private readonly ILoginService _loginService;
        private readonly IUserService _userService;
        private readonly IJwtService _jwtService;
        private readonly IFirmaService _firmaService;
        private readonly IConfiguration _configuration;

        private const string ACCESS_TOKEN_COOKIE = "access_token";
        private const string REFRESH_TOKEN_COOKIE = "refresh_token";
        private const string USER_INFO_COOKIE = "user_info";

        public LoginController(
            ILogger<LoginController> logger,
            ILoginService loginService,
            IUserService userService,
            IJwtService jwtService,
            IFirmaService firmaService,
            IConfiguration configuration)
        {
            _logger = logger;
            _loginService = loginService;
            _userService = userService;
            _jwtService = jwtService;
            _firmaService = firmaService;
            _configuration = configuration;
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginDTO login)
        {
            try
            {
                ClearAuthCookies();
                if (login == null)
                {
                    _logger.LogWarning("Login attempt with null credentials");
                    return BadRequest(new { error = "Invalid login data" });
                }

                // Valida le credenziali
                var isValid = await _loginService.LoginAsync(login);
                if (!isValid)
                {
                    _logger.LogWarning("Failed login attempt for email: {Email}", login.Email);
                    return Unauthorized(new { error = "Email or Password does not match or does not exist" });
                }

                // Ottieni i dati utente (UNA SOLA VOLTA con AsNoTracking)
                var userResponse = await _userService.GetUserByEmailAsync(login.Email);
                if (userResponse == null)
                {
                    _logger.LogError("User not found after successful login validation: {Email}", login.Email);
                    return StatusCode(500, new { error = "Internal server error" });
                }

                // Crea un oggetto User minimale per il JWT (SENZA caricare dal DB)
                var userForToken = new User
                {
                    IdPerson = userResponse.IdPerson,
                    Email = userResponse.Email,
                    Type = (int)userResponse.Type,
                    IdPersonNavigation = new Person
                    {
                        IdPerson = userResponse.IdPerson,
                        Name = userResponse.Name,
                        Surname = userResponse.Surname,
                        PhoneNumber = userResponse.PhoneNumber
                    }
                };

                // Genera i token
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
                var tokens = await _jwtService.GenerateTokensAsync(userForToken, ipAddress);

                // Imposta i cookie
                SetAuthCookies(tokens, userResponse);

                _logger.LogInformation("User {Email} logged in successfully from IP {IpAddress}",
                    login.Email, ipAddress);

                var firma = await _firmaService.GetLastFirma(userResponse.IdPerson);

                // Restituisce solo conferma e info non sensibili
                return Ok(new
                {
                    message = "Login successful",
                    user = new
                    {
                        userResponse.IdPerson,
                        userResponse.Email,
                        userResponse.Type,
                        userResponse.Name,
                        userResponse.Surname,
                        userResponse.PhoneNumber
                    },
                    lastFirma = firma != null ? new
                    {
                        entrata = firma.Entrata,
                        uscita = firma.Uscita
                    } : null
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
        public async Task<IActionResult> RefreshToken()
        {
            try
            {
                // Legge il refresh token dal cookie
                var refreshToken = Request.Cookies[REFRESH_TOKEN_COOKIE];

                if (string.IsNullOrEmpty(refreshToken))
                {
                    _logger.LogWarning("Refresh token attempt with empty token");
                    return BadRequest(new { error = "Refresh token is required" });
                }

                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
                var tokens = await _jwtService.RefreshTokenAsync(refreshToken, ipAddress);

                if (tokens == null)
                {
                    // Cancella i cookie se il refresh fallisce
                    ClearAuthCookies();
                    _logger.LogWarning("Invalid refresh token attempt from IP {IpAddress}", ipAddress);
                    return Unauthorized(new { error = "Invalid or expired refresh token" });
                }

                // Aggiorna i cookie con i nuovi token
                SetTokenCookies(tokens);

                _logger.LogInformation("Token refreshed successfully from IP {IpAddress}", ipAddress);

                return Ok(new { message = "Token refreshed successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during token refresh");
                return StatusCode(500, new { error = "Internal server error", details = ex.Message });
            }
        }

        [Authorize]
        [HttpPost("revoke")]
        public async Task<IActionResult> RevokeToken()
        {
            try
            {
                var refreshToken = Request.Cookies[REFRESH_TOKEN_COOKIE];

                if (string.IsNullOrEmpty(refreshToken))
                {
                    _logger.LogWarning("Revoke token attempt with empty token");
                    return BadRequest(new { error = "Refresh token is required" });
                }

                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
                var result = await _jwtService.RevokeTokenAsync(refreshToken, ipAddress);

                // Cancella sempre i cookie al revoke
                ClearAuthCookies();

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
        public async Task<IActionResult> Logout()
        {
            return await RevokeToken();
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentUser()
        {
            try
            {
                var userInfoCookie = Request.Cookies[USER_INFO_COOKIE];
                if (!string.IsNullOrEmpty(userInfoCookie))
                {
                    var userInfo = JsonSerializer.Deserialize<object>(Uri.UnescapeDataString(userInfoCookie));
                    return Ok(userInfo);
                }

                return NotFound(new { error = "User info not found" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current user");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [AllowAnonymous]
        [HttpGet("HealthCheck")]
        public IActionResult HealthCheck()
        {
            return Ok(new { status = "API is healthy" });
        }

        #region Cookie Helpers

        private void SetAuthCookies(JwtTokenResponse tokens, UserResponseDTO user)
        {
            SetTokenCookies(tokens);

            // Cookie user_info - NON HttpOnly (leggibile da JS)
            var userInfo = new
            {
                user.IdPerson,
                user.Email,
                user.Type,
                user.Name,
                user.Surname,
                user.PhoneNumber
            };

            var userInfoJson = JsonSerializer.Serialize(userInfo);

            var isProduction = !string.Equals(
                Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"),
                "Development",
                StringComparison.OrdinalIgnoreCase);

            Response.Cookies.Append(USER_INFO_COOKIE, Uri.EscapeDataString(userInfoJson), new CookieOptions
            {
                HttpOnly = false,  // Leggibile da JavaScript
                Secure = isProduction,
                SameSite = SameSiteMode.Strict,
                Expires = tokens.RefreshTokenExpiration,
                Path = "/"
            });
        }

        private void SetTokenCookies(JwtTokenResponse tokens)
        {
            var isProduction = !string.Equals(
                Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"),
                "Development",
                StringComparison.OrdinalIgnoreCase);

            // Cookie access_token - HttpOnly
            Response.Cookies.Append(ACCESS_TOKEN_COOKIE, tokens.AccessToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = isProduction,  // true in produzione, false in sviluppo per HTTP
                SameSite = SameSiteMode.Strict,
                Expires = tokens.AccessTokenExpiration,
                Path = "/"
            });

            // Cookie refresh_token - HttpOnly
            Response.Cookies.Append(REFRESH_TOKEN_COOKIE, tokens.RefreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = isProduction,
                SameSite = SameSiteMode.Strict,
                Expires = tokens.RefreshTokenExpiration,
                Path = "/api/v1/Login"  // Solo per gli endpoint di refresh/logout
            });
        }

        private void ClearAuthCookies()
        {
            var isProduction = !string.Equals(
                Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"),
                "Development",
                StringComparison.OrdinalIgnoreCase);

            Response.Cookies.Delete(ACCESS_TOKEN_COOKIE, new CookieOptions
            {
                HttpOnly = true,
                Secure = isProduction,
                SameSite = SameSiteMode.Strict,
                Path = "/"
            });

            Response.Cookies.Delete(USER_INFO_COOKIE, new CookieOptions
            {
                HttpOnly = false,
                Secure = isProduction,
                SameSite = SameSiteMode.Strict,
                Path = "/"
            });

            Response.Cookies.Delete(REFRESH_TOKEN_COOKIE, new CookieOptions
            {
                HttpOnly = true,
                Secure = isProduction,
                SameSite = SameSiteMode.Strict,
                Path = "/api/v1/Login"
            });
        }

        #endregion
    }
}