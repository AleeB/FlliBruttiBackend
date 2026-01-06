using System;
using FlliBrutti.Backend.Application.IContext;
using FlliBrutti.Backend.Application.IServices;
using FlliBrutti.Backend.Application.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FlliBrutti.Backend.Application.Services;

public class UserService : IUserService
{
    private readonly IFlliBruttiContext _context;
    private readonly ILogger _logger;
    public UserService(IFlliBruttiContext context, ILogger<UserService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<bool> AddUserAsync(UserDTO user)
    {
        try
        {
            if (await _context.Users.AnyAsync(u => u.Email == user.Email))
            {
                _logger.LogWarning($"Attempted to add User with existing Email: {user.Email}");
                return false;
            }

            // 1️⃣ crea UNA Person
            var person = user.ToPerson();

            // 2️⃣ salvala
            await _context.People.AddAsync(person);
            await _context.SaveChangesAsync(); // 👈 qui nasce IdPerson

            // 3️⃣ usa l'IdPerson generato
            var newUser = new Core.Models.User
            {
                IdPerson = person.IdPerson,
                Type = user.Type,
                Email = user.Email,
                Password = PasswordHash.Hash(user.Password)
            };

            await _context.Users.AddAsync(newUser);
            await _context.SaveChangesAsync();

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                $"Error adding User with Person: {user.Name} | {user.Surname}");
            return false;
        }
    }
}
