using FlliBrutti.Backend.Application.IContext;
using FlliBrutti.Backend.Application.ICrittography;
using FlliBrutti.Backend.Application.IServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FlliBrutti.Backend.Application.Services
{
    public class LoginService : ILoginService
    {
        private readonly ILogger<LoginService> _logger;
        private readonly IFlliBruttiContext _context;
        private readonly IPasswordHash _passwordHash;

        public LoginService(
            ILogger<LoginService> logger,
            IFlliBruttiContext context,
            IPasswordHash passwordHash)
        {
            _logger = logger;
            _context = context;
            _passwordHash = passwordHash;
        }

        public async Task<bool> LoginAsync(Models.LoginDTO login)
        {
            try
            {
                // 🔥 IMPORTANTE: AsNoTracking perché è solo verifica
                // Non serve tracciare l'entità User per il login
                var user = await _context.Users
                    .AsNoTracking()
                    .Where(u => u.Email == login.Email)
                    .Select(u => new { u.Email, u.Password })
                    .FirstOrDefaultAsync();

                if (user == null)
                {
                    _logger.LogWarning("Login failed for Email: {Email} - user does not exist",
                        login.Email);
                    return false;
                }

                var isPasswordValid = await _passwordHash.VerifyPassword(user.Password, login.Password);

                if (!isPasswordValid)
                {
                    _logger.LogWarning("Login failed for Email: {Email} - incorrect password",
                        login.Email);
                    return false;
                }

                _logger.LogInformation("User logged in successfully: {Email}", login.Email);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for Email: {Email}", login.Email);
                return false;
            }
        }
    }
}
