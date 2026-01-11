using System;
using System.Collections.Generic;
using FlliBrutti.Backend.Core.Enums;

namespace FlliBrutti.Backend.Core.Models;

public partial class User
{
    public long IdPerson { get; set; }

    public int Type { get; set; } = 1;      // Default to EType.Dipendente

    public string Email { get; set; } = null!;

    public string Password { get; set; } = null!;

    public virtual ICollection<Firma> Firme { get; set; } = new List<Firma>();

    public virtual Person IdPersonNavigation { get; set; } = null!;

    public virtual ICollection<PreventivoNCC> PreventiviNcc { get; set; } = new List<PreventivoNCC>();
}
