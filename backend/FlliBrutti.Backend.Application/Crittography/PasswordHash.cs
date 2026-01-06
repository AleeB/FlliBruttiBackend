using Konscious.Security.Cryptography;
using System.Security.Cryptography;
using System.Text;

public static class PasswordHash
{
    public static string Hash(string password)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(16);

        var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password))
        {
            Salt = salt,
            DegreeOfParallelism = 4,   // CPU threads
            Iterations = 4,            // tempo
            MemorySize = 65536         // 64 MB (sicuro nel 2025)
        };

        byte[] hash = argon2.GetBytes(32);

        return Convert.ToBase64String(
            salt.Concat(hash).ToArray()
        );
    }

    public static bool Verify(string password, string storedHash)
    {
        byte[] fullHash = Convert.FromBase64String(storedHash);

        byte[] salt = fullHash[..16];
        byte[] hash = fullHash[16..];

        var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password))
        {
            Salt = salt,
            DegreeOfParallelism = 4,
            Iterations = 4,
            MemorySize = 65536
        };

        byte[] computed = argon2.GetBytes(32);

        return CryptographicOperations.FixedTimeEquals(hash, computed);
    }

}