using FlliBrutti.Backend.Application.IServices;
using FlliBrutti.Backend.Application.Models;
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
        private readonly ILogger _logger;
        private readonly ILoginService _loginService;
        
        private readonly IUserService _userService;

        public LoginController(ILogger<LoginController> logger, ILoginService service, IUserService userService)
        {
            _logger = logger;
            _loginService = service;
            _userService = userService;
        }

        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO login)
        {
            if (login == null)
            {
                throw new ArgumentNullException(nameof(login));
            }
            var result = await _loginService.LoginAsync(login);
            if (result)
            {
                var user = await _userService.GetUserByEmailAsync(login.Email);
                return Ok(user);
            }
            return BadRequest("Email or Password does not match or does not exist");
        }
    }
}
