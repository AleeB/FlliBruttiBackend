using FlliBrutti.Backend.Application.Models;
using FlliBrutti.Backend.Application.Responses;
using FlliBrutti.Backend.Core.Enums;

namespace FlliBrutti.Backend.Application.IServices;

public interface IUserService
{
    public Task<bool> AddUserAsync(UserDTO user);
    public Task<UserResponseDTO> GetUserByEmailAsync(string email);
    public Task<UserResponseDTO> UpdatePasswordAsync(LoginDTO login);
    public Task<UserResponseDTO> UpdateTypeAsync(long idUser, EType type);
}
