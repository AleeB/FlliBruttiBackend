using FlliBrutti.Backend.Application.Crittography;
using FlliBrutti.Backend.Application.ICrittography;
using System.Runtime.CompilerServices;
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
            bool res;
            res = AreEqual("password123");
            Assert.True(res);
            res = AreEqual("ciao");
            Assert.True(res);
        }

        private bool AreEqual(string pw)
        {
            var hashed = _passwordHash.EncryptPassword(pw);
            var isValid = _passwordHash.VerifyPassword(hashed, pw);
            return isValid;
        }
    }
}