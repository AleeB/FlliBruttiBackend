using FlliBrutti.Backend.Application.IServices;
using FlliBrutti.Backend.Application.Models;
using FlliBrutti.Backend.Application.Responses;
using FlliBrutti.Backend.Core.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FlliBrutti.Backend.API.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly IUserService _userService;

        public UserController(ILogger<UserController> logger, IUserService userService)
        {
            _logger = logger;
            _userService = userService;
        }

        [Authorize(Roles = "1")]        //Admin
        [HttpPost]
        public async Task<IActionResult> Add([FromBody] UserDTO user)
        {
            try
            {
                if (user == null)
                {
                    _logger.LogWarning("Add User called with null user data");
                    return BadRequest(new { error = "User data is required" });
                }

                if (string.IsNullOrWhiteSpace(user.Name) ||
                    string.IsNullOrWhiteSpace(user.Surname) ||
                    string.IsNullOrWhiteSpace(user.Email) ||
                    string.IsNullOrWhiteSpace(user.Password))
                {
                    _logger.LogWarning("Add User called with incomplete data");
                    return BadRequest(new { error = "All fields are required (Name, Surname, Email, Password)" });
                }

                _logger.LogInformation($"UserController Add called for User: {user.Name} {user.Surname}");
                var res = await _userService.AddUserAsync(user);

                if (!res)
                {
                    _logger.LogWarning($"Failed to add User: {user.Name} {user.Surname}");
                    return BadRequest(new { error = "Could not add User. Email might already exist." });
                }

                _logger.LogInformation($"User {user.Name} {user.Surname} added successfully");
                return Ok(new { message = "User added successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error adding User: {user?.Name} {user?.Surname}. Exception: {ex.Message}");
                return StatusCode(500, new { error = "Internal server error", details = ex.Message });
            }
        }

        [HttpPatch]
        [Route("UpdatePassword")]
        [AllowAnonymous]
        public async Task<IActionResult> UpdatePasswordOfUser([FromBody] LoginDTO login)
        {
            if (login == null)
            {
                _logger.LogWarning("UpdatePasswordOfUser called with null login data");
                return BadRequest("UpdatePassword Failed");
            }
            _logger.LogInformation($"UserController UpdatePasswordOfUser called for Email: {login.Email}");
            var res = await _userService.UpdatePasswordAsync(login);
            if (res != null)
            {
                _logger.LogInformation($"Password updated successfully for Email: {login.Email}");
                return Ok(res);
            }
            return BadRequest("UpdatePassword Failed");
        }

        [Authorize]
        [HttpPatch]
        [Route("UpdateType")]
        public async Task<IActionResult> UpdateType(EType type)
        {
            var user = Convert.ToInt64(User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value);
            if (user == 0)
            {
                _logger.LogWarning("UpdateTypeOfUser called with null data");
                return BadRequest("UpdateType Failed");
            }
            _logger.LogInformation($"UserController UpdateTypeOfUser called for IdPerson: {user}");
            UserResponseDTO res = await _userService.UpdateTypeAsync(user, type);
            if (res == null)
            {
                _logger.LogWarning($"Failed to update type for IdPerson: {user}");
                return BadRequest("UpdateType Failed");
            }
            _logger.LogInformation($"Type updated successfully for IdPerson: {res.Email}");
            return Ok(res);
        }
    }
}
