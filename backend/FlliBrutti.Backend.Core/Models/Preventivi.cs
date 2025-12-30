using System;
using System.Collections.Generic;

namespace FlliBrutti.Backend.Core.Models;

public partial class Preventivi
{
    public long IdPreventivi { get; set; }

    public string Description { get; set; } = null!;

    public double? Costo { get; set; }

    public sbyte? IsTodo { get; set; }

    public long? IdUser { get; set; }

    public long? IdUserNonAutenticato { get; set; }

    public virtual User? IdUserNavigation { get; set; }

    public virtual UsersNotAuthenticated? IdUserNonAutenticatoNavigation { get; set; }
}
