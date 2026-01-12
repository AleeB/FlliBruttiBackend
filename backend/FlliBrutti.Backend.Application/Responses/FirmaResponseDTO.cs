using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlliBrutti.Backend.Application.Responses
{
    public class FirmaResponseDTO
    {
        public long Idfirma { get; set; }
        public DateTime? Entrata { get; set; }
        public DateTime? Uscita { get; set; }
        public long? IdUser { get; set; }

        // Informazioni utente senza riferimenti ciclici
        public string? UserEmail { get; set; }
        public string? UserName { get; set; }
        public string? UserSurname { get; set; }
    }
}
