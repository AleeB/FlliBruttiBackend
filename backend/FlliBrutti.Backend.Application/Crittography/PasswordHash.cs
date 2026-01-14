using FlliBrutti.Backend.Application.ICrittography;
using Konscious.Security.Cryptography;
using System.Security.Cryptography;
using System.Text;

namespace FlliBrutti.Backend.Application.Crittography
{
    public class PasswordHash : IPasswordHash
    {
        private readonly string _secret;

        public PasswordHash(string secret)
        {
            _secret = secret;
        }

        const int SaltSize = 16; // 16 bytes
        const int HashSize = 32; // 32 bytes
        const int Iterations = 3;
        const int MemorySize = 65536; // 64 MB
        const int DegreeOfParallelism = 4;

        /// <summary>
        /// Cripta una password usando Argon2id con salt e secret
        /// </summary>
        /// <param name="password">La password in chiaro</param>
        /// <returns>L'hash in formato Base64</returns>
        public string EncryptPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
                throw new ArgumentException("La password non può essere vuota", nameof(password));

            if (string.IsNullOrEmpty(_secret))
                throw new ArgumentException("Il secret non può essere vuoto", nameof(_secret));

            // Genera un salt casuale
            byte[] salt = new byte[SaltSize];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            // Combina password e secret
            string passwordWithSecret = password + _secret;
            byte[] passwordBytes = Encoding.UTF8.GetBytes(passwordWithSecret);

            // Genera l'hash con Argon2id
            byte[] hash = HashPassword(passwordBytes, salt);

            // Combina salt e hash per memorizzarli insieme
            byte[] hashWithSalt = new byte[SaltSize + HashSize];
            Array.Copy(salt, 0, hashWithSalt, 0, SaltSize);
            Array.Copy(hash, 0, hashWithSalt, SaltSize, HashSize);

            // Restituisce in Base64
            return Convert.ToBase64String(hashWithSalt);
        }

        /// <summary>
        /// Verifica se una password corrisponde all'hash memorizzato
        /// </summary>
        /// <param name="hashExisting">L'hash memorizzato (Base64)</param>
        /// <param name="passwordToVerify">La password da verificare</param>
        /// <returns>True se la password è corretta</returns>
        public bool VerifyPassword(string hashExisting, string passwordToVerify)
        {
            if (string.IsNullOrEmpty(hashExisting))
                throw new ArgumentException("L'hash non può essere vuoto", nameof(hashExisting));

            if (string.IsNullOrEmpty(passwordToVerify))
                throw new ArgumentException("La password non può essere vuota", nameof(passwordToVerify));

            if (string.IsNullOrEmpty(_secret))
                throw new ArgumentException("Il secret non può essere vuoto", nameof(_secret));

            try
            {
                // Decodifica l'hash memorizzato
                byte[] hashWithSalt = Convert.FromBase64String(hashExisting);

                if (hashWithSalt.Length != SaltSize + HashSize)
                    return false;

                // Estrae salt e hash
                byte[] salt = new byte[SaltSize];
                byte[] storedHash = new byte[HashSize];
                Array.Copy(hashWithSalt, 0, salt, 0, SaltSize);
                Array.Copy(hashWithSalt, SaltSize, storedHash, 0, HashSize);

                // Combina password e secret
                string passwordWithSecret = passwordToVerify + _secret;
                byte[] passwordBytes = Encoding.UTF8.GetBytes(passwordWithSecret);

                // Genera l'hash con lo stesso salt
                byte[] newHash = HashPassword(passwordBytes, salt);

                // Confronta gli hash in modo sicuro (constant-time)
                return CryptographicOperations.FixedTimeEquals(storedHash, newHash);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Metodo privato che esegue l'hashing con Argon2id
        /// </summary>
        private static byte[] HashPassword(byte[] password, byte[] salt)
        {
            using (var argon2 = new Argon2id(password))
            {
                argon2.Salt = salt;
                argon2.DegreeOfParallelism = DegreeOfParallelism;
                argon2.MemorySize = MemorySize;
                argon2.Iterations = Iterations;

                return argon2.GetBytes(HashSize);
            }
        }

    }
}