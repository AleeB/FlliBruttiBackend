using FlliBrutti.Backend.Application.IServices;
using FlliBrutti.Backend.Application.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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

        [HttpPost]
        [Route("Add")]
        public async Task<IActionResult> Add([FromBody] UserDTO user)
        {
            _logger.LogInformation($"UserController Add called for User: {user.Name} {user.Surname}");
            var res = await _userService.AddUserAsync(user);
            if (!res)
            {
                _logger.LogWarning($"Failed to add User: {user.Name} {user.Surname}");
                return BadRequest("Could not add User.");
            }
            return Ok();
        }
    }
}
