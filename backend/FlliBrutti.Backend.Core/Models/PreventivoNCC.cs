using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace FlliBrutti.Backend.Core.Models;

public partial class PreventivoNCC
{
    public long IdPreventivo { get; set; }

    public string Description { get; set; } = null!;

    public double? Costo { get; set; }

    public bool IsTodo { get; set; } = true;

    public string? Partenza { get; set; }

    public string? Arrivo { get; set; }

    public long? IdUser { get; set; }

    public long? IdUserNonAutenticato { get; set; }

    public virtual User? IdUserNavigation { get; set; }

    public virtual UserNotAuthenticated? IdUserNonAutenticatoNavigation { get; set; }

    public virtual ICollection<PreventivoExtra> PreventivoExtra { get; set; } = new List<PreventivoExtra>();
}
