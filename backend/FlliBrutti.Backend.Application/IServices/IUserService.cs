using System;
using FlliBrutti.Backend.Application.Models;
using FlliBrutti.Backend.Application.Responses;

namespace FlliBrutti.Backend.Application.IServices;

public interface IUserService
{
    public Task<bool> AddUserAsync(UserDTO user);
    public Task<object> GetUserByEmailAsync(string email);
    public Task<UserResponseDTO> UpdatePasswordAsync(LoginDTO login);
}
