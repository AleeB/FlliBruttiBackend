using System;
using FlliBrutti.Backend.Core.Enums;
using FlliBrutti.Backend.Core.Models;

namespace FlliBrutti.Backend.Application.Models;

public class UserDTO : PersonDTO
{
    public required EType Type { get; set; } = EType.Dipendente;
    public required string Email { get; set; } = null!;
    public required string Password { get; set; } = null!;

    internal User ToUser()
    {
        return new User
        {
            Type = Type,
            Email = Email,
            Password = Password
        };
    }

    internal Person ToPerson()
    {
        return new Person
        {
            
            Name = Name,
            Surname = Surname,
            DOB = DOB
        };
    }
}
