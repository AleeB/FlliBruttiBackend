using System;
using FlliBrutti.Backend.Application.Models;

namespace FlliBrutti.Backend.Application.IServices;

public interface IUserService
{
    public Task<bool> AddUserAsync(UserDTO user);
}
