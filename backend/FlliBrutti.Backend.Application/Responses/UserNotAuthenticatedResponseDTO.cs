namespace FlliBrutti.Backend.Application.Responses
{
    public class UserNotAuthenticatedResponseDTO
    {
        public long IdPerson { get; set; }
        public string Email { get; set; } = null!;
        public string Ip { get; set; } = null!;

        // Dati persona senza riferimenti ciclici
        public string? Name { get; set; }
        public string? Surname { get; set; }
        public string? PhoneNumber { get; set; }

        // Statistiche
        public int TotalPreventiviCount { get; set; }
    }
}
