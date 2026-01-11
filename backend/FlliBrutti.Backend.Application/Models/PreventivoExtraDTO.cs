
using FlliBrutti.Backend.Core.Models;

namespace FlliBrutti.Backend.Application.Models
{
    public class PreventivoExtraDTO
    {
        public required double Costo { get; set; }
        public required string Description { get; set; }

        internal PreventivoExtra ToModel()
        {
            return new PreventivoExtra
            {
                Costo = this.Costo,
                Description = this.Description
            };
        }
    }
}