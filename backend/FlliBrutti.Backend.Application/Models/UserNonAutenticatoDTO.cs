using System;

namespace FlliBrutti.Backend.Application.Models;

public class UserNonAutenticatoDTO : PersonDTO
{
    public required string Ip { get; set; } = null!;
    public required string Email { get; set; } = null!;
}
