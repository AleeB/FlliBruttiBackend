using System;

namespace FlliBrutti.Backend.Core.Entities;

public class User
{

    public long IdUser { get; set; }
    public EType typeUser { get; set; } = EType.Dipendente;
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";      //usare Konscious.Security.Cryptography.Argon2 per hashare la password


}
