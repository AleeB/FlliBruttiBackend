using FlliBrutti.Backend.Application.IContext;
using FlliBrutti.Backend.Application.ICrittography;
using FlliBrutti.Backend.Application.IServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlliBrutti.Backend.Application.Services
{
    public class LoginService : ILoginService
    {
        private readonly ILogger _logger;
        private readonly IFlliBruttiContext _context;
        private readonly IPasswordHash _passwordHash;

        public LoginService(ILogger<LoginService> logger, IFlliBruttiContext context, IPasswordHash passwordHash)
        {
            _logger = logger;
            _context = context;
            _passwordHash = passwordHash;
        }

        public async Task<bool> LoginAsync(Models.LoginDTO login)
        {
            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == login.Email);
                if (user == null && user == default)
                {
                    _logger.LogWarning($"Login failed for Email: {login.Email} it does not exist on db");
                    return false;
                }
                _passwordHash.VerifyPassword(login.Password, user.Password);
                _logger.LogInformation($"User logged in successfully: {login.Email}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error during login for Email: {login.Email}. Exception: {ex.Message}");
                return false;
            }
        }
    }
}
