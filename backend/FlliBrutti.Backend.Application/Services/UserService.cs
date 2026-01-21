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
    private readonly ILogger<UserService> _logger;
    private readonly IPasswordHash _passwordHash;

    public UserService(
        IFlliBruttiContext context,
        ILogger<UserService> logger,
        IPasswordHash passwordHash)
    {
        _context = context;
        _logger = logger;
        _passwordHash = passwordHash;
    }

    public async Task<bool> AddUserAsync(UserDTO user)
    {
        try
        {
            // AsNoTracking per la verifica esistenza
            if (await _context.Users.AsNoTracking().AnyAsync(u => u.Email == user.Email))
            {
                _logger.LogWarning("Attempted to add User with existing Email: {Email}", user.Email);
                return false;
            }

            // 1️⃣ crea UNA Person
            var person = user.ToPerson();

            // 2️⃣ salvala
            await _context.People.AddAsync(person);
            await _context.SaveChangesAsync();

            // 3️⃣ usa l'IdPerson generato
            var newUser = new Core.Models.User
            {
                IdPerson = person.IdPerson,
                Type = (int)user.Type,
                Email = user.Email,
                Password = await _passwordHash.EncryptPassword(user.Password)
            };

            await _context.Users.AddAsync(newUser);
            await _context.SaveChangesAsync();

            _logger.LogInformation("User {Email} added successfully", user.Email);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding User: {Name} {Surname}",
                user.Name, user.Surname);
            return false;
        }
    }

    public async Task<UserResponseDTO> GetUserByEmailAsync(string email)
    {
        _logger.LogInformation("Fetching User with Email: {Email}", email);

        if (string.IsNullOrEmpty(email))
        {
            _logger.LogWarning("Email provided is null or empty");
            throw new ArgumentNullException(nameof(email), "email is null");
        }

        var user = await _context.Users
            .AsNoTracking()
            .Where(u => u.Email == email)
            .Select(u => new UserResponseDTO
            {
                Email = u.Email,
                IdPerson = u.IdPerson,
                Type = (EType)u.Type,
                PhoneNumber = u.IdPersonNavigation.PhoneNumber,
                Name = u.IdPersonNavigation.Name,
                Surname = u.IdPersonNavigation.Surname
            })
            .FirstOrDefaultAsync();

        if (user == null)
        {
            _logger.LogWarning("User with Email: {Email} not found", email);
            return null!;
        }

        return user;
    }

    public async Task<UserResponseDTO> UpdatePasswordAsync(LoginDTO login)
    {
        _logger.LogInformation("Updating password for Email: {Email}", login.Email);

        // Non usare AsNoTracking perché dobbiamo modificare
        var user = await _context.Users
            .Include(u => u.IdPersonNavigation)
            .FirstOrDefaultAsync(u => u.Email == login.Email);

        if (user == null)
        {
            _logger.LogWarning("User with Email: {Email} not found", login.Email);
            return null!;
        }

        user.Password = await _passwordHash.EncryptPassword(login.Password);

        // Non serve chiamare Update se l'entità è già tracciata
        await _context.SaveChangesAsync();

        _logger.LogInformation("Password updated successfully for Email: {Email}", login.Email);

        return new UserResponseDTO
        {
            Email = login.Email,
            IdPerson = user.IdPerson,
            Type = (EType)user.Type,
            Name = user.IdPersonNavigation.Name,
            Surname = user.IdPersonNavigation.Surname,
            PhoneNumber = user.IdPersonNavigation.PhoneNumber
        };
    }

    public async Task<UserResponseDTO> UpdateTypeAsync(long idUser, EType type)
    {
        _logger.LogInformation("Updating type for User IdPerson: {IdPerson} to Type: {Type}", idUser, type);

        var userToUpdate = await _context.Users
            .Include(u => u.IdPersonNavigation)
            .FirstOrDefaultAsync(u => u.IdPerson == idUser);

        if (userToUpdate == null || userToUpdate == default)
        {
            _logger.LogWarning($"User with Id: {idUser} not found");
            return null!;
        }
        userToUpdate.Type = (int)type;
        await _context.SaveChangesAsync();
        _logger.LogInformation($"User type updated successfully for Email: {idUser}");
        return new UserResponseDTO
        {
            Email = userToUpdate.Email,
            IdPerson = userToUpdate.IdPerson,
            Type = (EType)userToUpdate.Type,
            Name = userToUpdate.IdPersonNavigation.Name,
            Surname = userToUpdate.IdPersonNavigation.Surname,
            PhoneNumber = userToUpdate.IdPersonNavigation.PhoneNumber
        };
    }
}
