using System;
using System.Collections.Generic;

namespace FlliBrutti.Backend.Core.Models;

public partial class Person
{
    public long IdPerson { get; set; }

    public string? Name { get; set; }

    public string? Surname { get; set; }

    public DateOnly? Dob { get; set; }

    public virtual User? User { get; set; }

    public virtual UsersNotAuthenticated? UsersNotAuthenticated { get; set; }
}
