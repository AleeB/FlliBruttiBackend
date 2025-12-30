using System;

namespace FlliBrutti.Backend.Core.Entities;

public interface IPerson
{
    public long IdPerson { get; set; }
    public string Name { get; set; }
    public string Surname { get; set; }
    public DateTime DOB { get; set; }

}
