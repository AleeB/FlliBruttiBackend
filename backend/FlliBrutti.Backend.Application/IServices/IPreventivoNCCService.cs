using System;
using FlliBrutti.Backend.Application.Models;
using FlliBrutti.Backend.Core.Models;

namespace FlliBrutti.Backend.Application.IServices;

public interface IPreventivoNCCService
{
    Task<(bool, string )> AddPreventivoNCCAsync(PreventivoNCCDTO preventivo);
    Task<IEnumerable<PreventivoNCC>> GetPreventiviToExamineAsync(bool isTodo);
    Task<PreventivoNCC> GetPreventivoByIdAsync(long id);
    Task<bool> UpdatePreventivoNCCAsync(long id, PreventivoUpdateNCCDTO preventivo);

}
