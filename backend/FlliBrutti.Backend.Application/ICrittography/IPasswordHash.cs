namespace FlliBrutti.Backend.Application.ICrittography
{
    public interface IPasswordHash
    {
        public Task<string> EncryptPassword(string password);

        public Task<bool> VerifyPassword(string hashExisting, string passwordToVerify);
    }
}
