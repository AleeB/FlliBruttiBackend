using System;

namespace FlliBrutti.Backend.Core.Entities;

public class Firma
{
    public long Id { get; set; }
    public DateTime EntranceDate { get; set; }
    public DateTime? ExitDate { get; set; }
    public long UserId { get; set; }
    public virtual required User User { get; set; }
}
