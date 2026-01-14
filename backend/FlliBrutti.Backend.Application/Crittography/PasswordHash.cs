using FlliBrutti.Backend.Application.ICrittography;
using Konscious.Security.Cryptography;
using System.Security.Cryptography;
using System.Text;

namespace FlliBrutti.Backend.Application.Crittography
{
    public class PasswordHash : IPasswordHash
    {
        private const int SaltSize = 16; // 128 bit
        private const int HashSize = 32; // 256 bit
        private const int Iterations = 4;
        private const int MemorySize = 65536; // 64 MB
        private const int DegreeOfParallelism = 1;

        private readonly byte[] _secretKey;

        public PasswordHash(string secret)
        {
            if (string.IsNullOrEmpty(secret))
            {
                throw new ArgumentNullException(nameof(secret), "Secret cannot be null or empty");
            }
            _secretKey = Encoding.UTF8.GetBytes(secret);
        }

        public string EncryptPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
            {
                throw new ArgumentNullException(nameof(password), "Password cannot be null or empty");
            }

            // Genera un salt casuale
            byte[] salt = new byte[SaltSize];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            // Hash della password con Argon2
            byte[] hash = HashPasswordWithSalt(password, salt);

            // Combina salt + hash
            byte[] hashWithSalt = new byte[SaltSize + HashSize];
            Array.Copy(salt, 0, hashWithSalt, 0, SaltSize);
            Array.Copy(hash, 0, hashWithSalt, SaltSize, HashSize);

            // Restituisce come Base64
            return Convert.ToBase64String(hashWithSalt);
        }

        public bool VerifyPassword(string hashedPassword, string passwordToVerify)
        {
            if (string.IsNullOrEmpty(hashedPassword) || string.IsNullOrEmpty(passwordToVerify))
            {
                return false;
            }

            try
            {
                // Decodifica l'hash Base64
                byte[] hashWithSalt = Convert.FromBase64String(hashedPassword);

                // Verifica che la lunghezza sia corretta
                if (hashWithSalt.Length != SaltSize + HashSize)
                {
                    return false;
                }

                // Estrae salt e hash
                byte[] salt = new byte[SaltSize];
                byte[] hash = new byte[HashSize];
                Array.Copy(hashWithSalt, 0, salt, 0, SaltSize);
                Array.Copy(hashWithSalt, SaltSize, hash, 0, HashSize);

                // Hash della password da verificare con lo stesso salt
                byte[] testHash = HashPasswordWithSalt(passwordToVerify, salt);

                // Confronto constant-time per evitare timing attacks
                return CryptographicOperations.FixedTimeEquals(hash, testHash);
            }
            catch
            {
                return false;
            }
        }

        private byte[] HashPasswordWithSalt(string password, byte[] salt)
        {
            using (var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password)))
            {
                argon2.Salt = salt;
                argon2.DegreeOfParallelism = DegreeOfParallelism;
                argon2.MemorySize = MemorySize;
                argon2.Iterations = Iterations;
                argon2.KnownSecret = _secretKey;

                return argon2.GetBytes(HashSize);
            }
        }
    }
}