namespace FlliBrutti.Backend.Application.Responses
{
    public class PersonResponseDTO
    {
        public long IdPerson { get; set; }
        public string? Name { get; set; }
        public string? Surname { get; set; }
        public DateOnly? DOB { get; set; }

        // Flag per indicare se ha un account utente o non autenticato
        public bool HasUserAccount { get; set; }
        public bool HasNonAuthenticatedAccount { get; set; }
    }
}
