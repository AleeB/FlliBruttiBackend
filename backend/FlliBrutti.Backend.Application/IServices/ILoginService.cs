using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlliBrutti.Backend.Application.IServices
{
    public interface ILoginService
    {
        public Task<bool> LoginAsync(Models.LoginDTO login);
    }
}
