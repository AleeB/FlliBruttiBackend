using FlliBrutti.Backend.Application.Extensions;
using FlliBrutti.Backend.Application.IContext;
using FlliBrutti.Backend.Application.IServices;
using FlliBrutti.Backend.Application.Models;
using FlliBrutti.Backend.Application.Responses;
using FlliBrutti.Backend.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FlliBrutti.Backend.Application.Services;

public class PersonService : IPersonService
{
    private readonly IFlliBruttiContext _context;
    private readonly ILogger _logger;

    public PersonService(IFlliBruttiContext context, ILogger<PersonService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<PersonResponseDTO> GetPersonById(long id)
    {
        var person = await _context.People
            .Include(p => p.User)
            .Include(p => p.UserNotAuthenticated)
            .FirstOrDefaultAsync(p => p.IdPerson == id);

        if (person == null)
        {
            _logger.LogWarning($"Person with Id: {id} not found.");
            return null!;
        }

        return person.ToResponseDTO();
    }

    public async Task<bool> UpdatePerson(Person person)
    {
        var existingPerson = await _context.People.FindAsync(person.IdPerson);
        if (existingPerson == null)
        {
            _logger.LogWarning($"Person with Id: {person.IdPerson} not found.");
            return false;
        }

        try
        {
            _context.People.Update(person);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error updating Person with Id: {person.IdPerson}. Exception: {ex.Message}");
            return false;
        }
        return true;
    }

    public async Task<bool> DeletePerson(long id)
    {
        var person = await _context.People.FindAsync(id);
        if (person == null)
        {
            _logger.LogWarning($"Person with Id: {id} not found.");
            return false;
        }

        _context.People.Remove(person);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> AddPerson(PersonDTO person)
    {
        _logger.LogInformation($"Adding new Person: {person.Name} | {person.Surname}");
        try
        {
            _context.People.Add(new Person
            {
                Name = person.Name,
                Surname = person.Surname,
                PhoneNumber = person.PhoneNumber
            });
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error adding new Person: {person.Name} | {person.Surname}. Exception: {ex.Message}");
            return false;
        }
    }
}