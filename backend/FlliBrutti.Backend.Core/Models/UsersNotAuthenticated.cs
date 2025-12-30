using System;
using System.Collections.Generic;

namespace FlliBrutti.Backend.Core.Models;

public partial class UsersNotAuthenticated
{
    public long IdPerson { get; set; }

    public string Ip { get; set; } = null!;

    public virtual Person IdPersonNavigation { get; set; } = null!;

    public virtual ICollection<Preventivo> Preventivi { get; set; } = new List<Preventivo>();
}
