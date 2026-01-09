using System;
using System.Collections.Generic;

namespace FlliBrutti.Backend.Core.Models;

public partial class UserNotAuthenticated
{
    public long IdPerson { get; set; }

    public string Ip { get; set; } = null!;

    public string Email { get; set; } = null!;

    public virtual Person IdPersonNavigation { get; set; } = null!;

    public virtual ICollection<PreventivoNCC> PreventiviNccs { get; set; } = new List<PreventivoNCC>();
}
