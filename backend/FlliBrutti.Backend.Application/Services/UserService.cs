using System;
using FlliBrutti.Backend.Application.IContext;
using FlliBrutti.Backend.Application.ICrittography;
using FlliBrutti.Backend.Application.IServices;
using FlliBrutti.Backend.Application.Models;
using FlliBrutti.Backend.Application.Responses;
using FlliBrutti.Backend.Core.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FlliBrutti.Backend.Application.Services;

public class UserService : IUserService
{
    private readonly IFlliBruttiContext _context;
    private readonly ILogger _logger;
    private readonly IPasswordHash _passwordHash;
    public UserService(IFlliBruttiContext context, ILogger<UserService> logger, IPasswordHash passwordHash)
    {
        _context = context;
        _logger = logger;
        _passwordHash = passwordHash;
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
                Type = (int)user.Type,
                Email = user.Email,
                Password = _passwordHash.EncryptPassword(user.Password)
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

    public async Task<object> GetUserByEmailAsync(string email)
    {
        _logger.LogInformation($"Fetching User with Email: {email}");
        if (string.IsNullOrEmpty(email))
        {
            _logger.LogInformation($"Email provided is null: {nameof(email)}");
            throw new ArgumentNullException("email is null");
        }
        var user = await _context.Users
            .Include(u => u.IdPersonNavigation)
            .FirstOrDefaultAsync(u => u.Email == email);
        if (user == null)
        {
            _logger.LogWarning($"User with Email: {email} not found");
            return null!;
        }
        return new
        {
            Email = email,
            IdPerson = user.IdPerson,
            Type = (EType)user.Type,
            DOB = user.IdPersonNavigation.DOB,
            Name = user.IdPersonNavigation.Name,
            Surname = user.IdPersonNavigation.Surname
        };
    }

    public async Task<bool> UpdatePassword(string email, string password)
    {
        if (password == null)
        {
            throw new ArgumentNullException($"Password was null: {password}");
        }
        var res = _context.Users.FirstOrDefaultAsync(u => u.Email == email).Result;
        if (res == null)
        {
            _logger.LogWarning($"User was not found email: {email}");
            return false;
        }
        res.Password = _passwordHash.EncryptPassword(password);
        _context.Users.Update(res);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<UserResponseDTO> UpdatePasswordAsync(LoginDTO login)
    {
        _logger.LogInformation($"Updating password for Email: {login.Email}");
        var user = await _context.Users.Where(u => u.Email == login.Email)
            .Include(u => u.IdPersonNavigation)
            .FirstOrDefaultAsync();
        if (user == null && user == default)
        {
            _logger.LogWarning($"User with Email: {login.Email} not found");
            return null!;
        }
        user.Password = _passwordHash.EncryptPassword(login.Password);
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
        _logger.LogInformation($"Password updated successfully for Email: {login.Email}");
        return new UserResponseDTO
        {
            Email = login.Email,
            IdPerson = user.IdPerson,
            Type = (EType)user.Type,
            Name = user.IdPersonNavigation.Name,
            Surname = user.IdPersonNavigation.Surname,
            DOB = user.IdPersonNavigation.DOB
        };
    }
}