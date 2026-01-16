using FlliBrutti.Backend.Application.Crittography;
using FlliBrutti.Backend.Application.ICrittography;
using Xunit.Abstractions;

namespace FlliBrutti.Backend.Test
{
    public class PasswordHashTest
    {
        private readonly ITestOutputHelper _output;
        private readonly IPasswordHash _passwordHash;

        public PasswordHashTest(ITestOutputHelper output)
        {
            _output = output;
            var secret = "Secret-Di-Sviluppo";
            _passwordHash = new PasswordHash(secret);
        }

        [Fact]
        public void EncryptAndDecryptTest()
        {
            // Test con password semplici
            Assert.True(AreEqual("password123"), "Failed to verify 'password123'");
            Assert.True(AreEqual("ciao"), "Failed to verify 'ciao'");

            _output.WriteLine("✓ Basic password encryption and verification passed");
        }

        [Fact]
        public void DifferentPasswordsShouldNotMatch()
        {
            var hashed = _passwordHash.EncryptPassword("password123");
            var isValid = _passwordHash.VerifyPassword(hashed, "wrongpassword");

            Assert.False(isValid, "Different passwords should not match");
            _output.WriteLine("✓ Different passwords correctly rejected");
        }

        [Fact]
        public void SamePasswordShouldProduceDifferentHashes()
        {
            var hash1 = _passwordHash.EncryptPassword("testpassword");
            var hash2 = _passwordHash.EncryptPassword("testpassword");

            Assert.NotEqual(hash1, hash2);

            // Ma entrambi dovrebbero verificare correttamente
            Assert.True(_passwordHash.VerifyPassword(hash1, "testpassword"));
            Assert.True(_passwordHash.VerifyPassword(hash2, "testpassword"));

            _output.WriteLine("✓ Salt randomization working correctly");
        }

        [Fact]
        public void ComplexPasswordsTest()
        {
            var complexPasswords = new[]
            {
                "MyP@ssw0rd!2024",
                "Sup3r$ecure#Pass",
                "Test_1234!@#$%",
                "àèéìòù€£¥",
                "パスワード123"
            };

            foreach (var password in complexPasswords)
            {
                Assert.True(AreEqual(password), $"Failed to verify complex password: {password}");
            }

            _output.WriteLine("✓ Complex passwords handled correctly");
        }

        [Fact]
        public void EmptyPasswordShouldThrowException()
        {
            Assert.Throws<ArgumentNullException>(() => _passwordHash.EncryptPassword(""));
            Assert.Throws<ArgumentNullException>(() => _passwordHash.EncryptPassword(null!));

            _output.WriteLine("✓ Empty/null passwords correctly rejected");
        }

        [Fact]
        public void InvalidHashShouldReturnFalse()
        {
            var result1 = _passwordHash.VerifyPassword("invalid_base64_string", "password");
            var result2 = _passwordHash.VerifyPassword("dGVzdA==", "password"); // too short

            Assert.False(result1, "Invalid hash should return false");
            Assert.False(result2, "Short hash should return false");

            _output.WriteLine("✓ Invalid hashes correctly handled");
        }

        [Fact]
        public void CaseSensitivityTest()
        {
            var hashed = _passwordHash.EncryptPassword("Password123");

            Assert.True(_passwordHash.VerifyPassword(hashed, "Password123"));
            Assert.False(_passwordHash.VerifyPassword(hashed, "password123"));
            Assert.False(_passwordHash.VerifyPassword(hashed, "PASSWORD123"));

            _output.WriteLine("✓ Password verification is case-sensitive");
        }

        [Fact]
        public void WhitespaceHandlingTest()
        {
            var hashed1 = _passwordHash.EncryptPassword("password");
            var hashed2 = _passwordHash.EncryptPassword(" password");
            var hashed3 = _passwordHash.EncryptPassword("password ");

            Assert.False(_passwordHash.VerifyPassword(hashed1, " password"));
            Assert.False(_passwordHash.VerifyPassword(hashed1, "password "));
            Assert.True(_passwordHash.VerifyPassword(hashed2, " password"));
            Assert.True(_passwordHash.VerifyPassword(hashed3, "password "));

            _output.WriteLine("✓ Whitespace is treated as part of password");
        }

        [Fact]
        public void LongPasswordTest()
        {
            var longPassword = new string('a', 1000);
            Assert.True(AreEqual(longPassword), "Failed to verify very long password");

            _output.WriteLine("✓ Long passwords handled correctly");
        }

        [Theory]
        [InlineData("admin123")]
        [InlineData("user@2024")]
        [InlineData("Test!Pass")]
        [InlineData("12345678")]
        public void TheoryBasedPasswordTest(string password)
        {
            Assert.True(AreEqual(password), $"Failed to verify password: {password}");
        }

        [Fact]
        public void PerformanceTest()
        {
            var password = "TestPassword123!";
            var iterations = 10;

            var startTime = DateTime.UtcNow;

            for (int i = 0; i < iterations; i++)
            {
                var hashed = _passwordHash.EncryptPassword(password);
                _passwordHash.VerifyPassword(hashed, password);
            }

            var elapsed = DateTime.UtcNow - startTime;
            var avgTime = elapsed.TotalMilliseconds / iterations;

            _output.WriteLine($"✓ Average time per hash+verify: {avgTime:F2}ms");

            // Argon2 dovrebbe richiedere un po' di tempo (è intenzionale per sicurezza)
            // ma non troppo per essere utilizzabile
            Assert.True(avgTime < 5000, "Hash operation took too long");
        }

        private bool AreEqual(string pw)
        {
            var hashed = _passwordHash.EncryptPassword(pw);
            var isValid = _passwordHash.VerifyPassword(hashed, pw);
            return isValid;
        }
    }

    public class LoginServiceIntegrationTest
    {
        private readonly ITestOutputHelper _output;

        public LoginServiceIntegrationTest(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void SimulateUserRegistrationAndLogin()
        {
            var secret = "Secret-Di-Sviluppo";
            var passwordHash = new PasswordHash(secret);

            // Simula registrazione
            var originalPassword = "MySecurePass123!";
            var hashedPassword = passwordHash.EncryptPassword(originalPassword);

            _output.WriteLine($"User registered with hashed password: {hashedPassword.Substring(0, 20)}...");

            // Simula login corretto
            var loginAttempt1 = passwordHash.VerifyPassword(hashedPassword, originalPassword);
            Assert.True(loginAttempt1, "Valid login should succeed");
            _output.WriteLine("✓ Valid login successful");

            // Simula login errato
            var loginAttempt2 = passwordHash.VerifyPassword(hashedPassword, "WrongPassword");
            Assert.False(loginAttempt2, "Invalid login should fail");
            _output.WriteLine("✓ Invalid login correctly rejected");
        }

        [Fact]
        public void SimulatePasswordChange()
        {
            var secret = "Secret-Di-Sviluppo";
            var passwordHash = new PasswordHash(secret);

            // Password originale
            var oldPassword = "OldPassword123";
            var oldHash = passwordHash.EncryptPassword(oldPassword);

            // Cambio password
            var newPassword = "NewPassword456";
            var newHash = passwordHash.EncryptPassword(newPassword);

            // La vecchia password non dovrebbe più funzionare con il nuovo hash
            Assert.False(passwordHash.VerifyPassword(newHash, oldPassword));

            // La nuova password dovrebbe funzionare con il nuovo hash
            Assert.True(passwordHash.VerifyPassword(newHash, newPassword));

            // Il vecchio hash dovrebbe ancora funzionare con la vecchia password
            Assert.True(passwordHash.VerifyPassword(oldHash, oldPassword));

            _output.WriteLine("✓ Password change simulation successful");
        }
    }
}