namespace FlliBrutti.Backend.Application.Responses
{
    public class PreventivoNCCResponseDTO
    {
        public long IdPreventivo { get; set; }
        public string Description { get; set; } = null!;
        public double? Costo { get; set; }
        public bool IsTodo { get; set; }
        public string? Partenza { get; set; }
        public string? Arrivo { get; set; }

        // Riferimento utente autenticato (se presente)
        public long? IdUser { get; set; }
        public string? UserEmail { get; set; }
        public string? UserName { get; set; }
        public string? UserSurname { get; set; }

        // Riferimento utente non autenticato (se presente)
        public long? IdUserNonAutenticato { get; set; }
        public string? NonAuthUserEmail { get; set; }
        public string? NonAuthUserName { get; set; }
        public string? NonAuthUserSurname { get; set; }
        public string? NonAuthUserIp { get; set; }

        // Lista extra senza riferimenti ciclici
        public ICollection<PreventivoExtraResponseDTO> Extra { get; set; } = new List<PreventivoExtraResponseDTO>();
        public string? NonAuthUserPhoneNumber { get; set; }
    }

    public class PreventivoExtraResponseDTO
    {
        public long Id { get; set; }
        public double Costo { get; set; }
        public string Description { get; set; } = null!;
        public long IdPreventivo { get; set; }
    }
}
