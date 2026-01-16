namespace FlliBrutti.Backend.Application.Models
{
    public class PreventivoUpdateNCCDTO
    {
        public required string Description { get; set; }

        public bool IsTodo { get; } = false;

        public long? IdUser { get; set; } = null;

        public string? Partenza { get; set; } = "";

        public string? Arrivo { get; set; } = "";

        public required long IdUserNonAutenticato { get; set; }

        public required double Costo { get; set; }

        public required ICollection<PreventivoExtraDTO> Extra { get; set; }

    }

}
