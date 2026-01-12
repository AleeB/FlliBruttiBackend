using System;
using FlliBrutti.Backend.Application.Models;
using FlliBrutti.Backend.Application.Responses;
using FlliBrutti.Backend.Core.Models;

namespace FlliBrutti.Backend.Application.IServices;

public interface IPersonService
{
    Task<PersonResponseDTO> GetPersonById(long id);
    Task<bool> UpdatePerson(Person person);
    Task<bool> DeletePerson(long id);
    Task<bool> AddPerson(PersonDTO person);
}
