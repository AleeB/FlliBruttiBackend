using System;
using FlliBrutti.Backend.Application.IContext;
using FlliBrutti.Backend.Application.IServices;
using FlliBrutti.Backend.Core.Models;
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

    public async Task<Person> GetPersonById(long id)
    {
        var person = await _context.People.FindAsync(id);
        if (person == null)
        {
            _logger.LogWarning($"Person with Id: {id} not found.");
            return null;
        }
        return person;
    }
}