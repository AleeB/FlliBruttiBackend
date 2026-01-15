using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlliBrutti.Backend.Application.ICrittography
{
    public interface IPasswordHash
    {
        public Task<string> EncryptPassword(string password);

        public Task<bool> VerifyPassword(string hashExisting, string passwordToVerify);
    }
}
