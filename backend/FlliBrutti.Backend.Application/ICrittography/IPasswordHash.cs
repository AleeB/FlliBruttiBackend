using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlliBrutti.Backend.Application.ICrittography
{
    public interface IPasswordHash
    {
        public string EncryptPassword(string password);

        public bool VerifyPassword(string hashExisting, string passwordToVerify);
    }
}
