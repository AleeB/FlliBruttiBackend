using System;
using System.Collections.Generic;
using FlliBrutti.Backend.Core.Entities;

namespace FlliBrutti.Backend.Core.Models;

public partial class User
{
    public long IdPerson { get; set; }

    public EType Type { get; set; } = EType.Dipendente;

    public string Email { get; set; } = null!;

    public string Password { get; set; } = null!;

    public virtual ICollection<Firma> Firme { get; set; } = new List<Firma>();

    public virtual Person IdPersonNavigation { get; set; } = null!;

    public virtual ICollection<Preventivo> Preventivi { get; set; } = new List<Preventivo>();
}
