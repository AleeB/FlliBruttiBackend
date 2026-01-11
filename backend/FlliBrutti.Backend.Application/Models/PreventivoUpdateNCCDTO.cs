using FlliBrutti.Backend.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
