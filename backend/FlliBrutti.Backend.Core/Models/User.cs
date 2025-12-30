using System;
using System.Collections.Generic;

namespace FlliBrutti.Backend.Core.Models;

public partial class User
{
    public long IdPerson { get; set; }

    public string Type { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Password { get; set; } = null!;

    public virtual ICollection<Firme> Firmes { get; set; } = new List<Firme>();

    public virtual Person IdPersonNavigation { get; set; } = null!;

    public virtual ICollection<Preventivi> Preventivis { get; set; } = new List<Preventivi>();
}
