namespace FlliBrutti.Backend.Application.Models;

public class PersonDTO
{
    public required string Name { get; set; } = null!;
    public required string Surname { get; set; } = null!;
    public required string PhoneNumber { get; set; }
}
