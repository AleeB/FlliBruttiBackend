using FlliBrutti.Backend.Core.Models;

namespace FlliBrutti.Backend.Application.Models;

public class UserNonAutenticatoDTO : PersonDTO
{
    public required string Ip { get; set; } = null!;
    public required string Email { get; set; } = null!;

    internal UserNotAuthenticated ToModel()
    {
        return new UserNotAuthenticated
        {
            Email = this.Email,
            Ip = this.Ip,
            IdPersonNavigation = new Person
            {
                Name = this.Name,
                Surname = this.Surname,
                PhoneNumber = this.PhoneNumber
            }
        };
    }
}
