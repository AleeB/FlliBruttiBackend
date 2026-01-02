using FlliBrutti.Backend.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlliBrutti.Backend.Application.IServices
{
    public interface IFirmaService
    {
        Task<bool> CreateFirma(long idUser);
        Task<bool> ExitFirma(long idFirma);
        IEnumerable<Firma> GetFirmaByIdUser(long idUser);
    }
}
