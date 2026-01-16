namespace FlliBrutti.Backend.Application.IServices
{
    public interface ILoginService
    {
        public Task<bool> LoginAsync(Models.LoginDTO login);
    }
}
