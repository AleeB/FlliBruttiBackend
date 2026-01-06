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
        Task<(bool, string)> CreateFirma(long idUser);
        Task<(bool, string)> ExitFirma(long idFirma, DateOnly date);
        Task<IEnumerable<Firma>> GetFirmaByIdUserAsync(long idUser);
    }
}
