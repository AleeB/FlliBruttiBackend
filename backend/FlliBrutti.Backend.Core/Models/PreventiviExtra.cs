using FlliBrutti.Backend.Core.Models;
using System;
using System.Collections.Generic;

namespace FlliBrutti.Backend.Core.Models;

public partial class PreventivoExtra
{
    public long Id { get; set; }

    public double Costo { get; set; }

    public string Description { get; set; } = null!;

    public long IdPreventivo { get; set; }

    public virtual PreventivoNCC IdPreventivoNavigation { get; set; } = null!;
}
