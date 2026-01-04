using System;
using FlliBrutti.Backend.Core.Models;

namespace FlliBrutti.Backend.Application.IServices;

public interface IPersonService
{
    Task<Person> GetPersonById(long id);
}
