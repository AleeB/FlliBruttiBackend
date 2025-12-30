using System;
using System.Collections.Generic;

namespace FlliBrutti.Backend.Core.Models;

public partial class Firma
{
    public long Idfirma { get; set; }

    public DateTime? Entrata { get; set; }

    public DateTime? Uscita { get; set; }

    public long? IdUser { get; set; }

    public virtual User? IdUserNavigation { get; set; }
}
