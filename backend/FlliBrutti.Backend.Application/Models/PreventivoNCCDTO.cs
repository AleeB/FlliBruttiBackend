using System;

namespace FlliBrutti.Backend.Application.Models;

public class PreventivoNCCDTO
{
    public required string Description { get; set; }

    // public bool IsTodo { get; set; } = true;

    // public long? IdUser { get; set; } = null;

    public string? Partenza { get; set; } = "";

    public string? Arrivo { get; set; } = "";

    public required long IdUserNonAutenticato { get; set; }
}
