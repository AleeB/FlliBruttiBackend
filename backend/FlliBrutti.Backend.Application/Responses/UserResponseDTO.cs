using FlliBrutti.Backend.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlliBrutti.Backend.Application.Responses
{
    public class UserResponseDTO
    {
        public long IdPerson { get; set; }
        public string Email { get; set; } = null!;
        public EType Type { get; set; }

        // Dati persona senza riferimenti ciclici
        public string? Name { get; set; }
        public string? Surname { get; set; }
        public DateOnly? DOB { get; set; }

        // Statistiche senza caricare tutte le entità
        public int TotalFirmeCount { get; set; }
        public int TotalPreventiviCount { get; set; }
    }
}
