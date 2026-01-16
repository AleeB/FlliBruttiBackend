using FlliBrutti.Backend.Application.Models;
using FlliBrutti.Backend.Application.Responses;

namespace FlliBrutti.Backend.Application.IServices;

public interface IPreventivoNCCService
{
    Task<(bool, string)> AddPreventivoNCCAsync(PreventivoNCCDTO preventivo);
    Task<IEnumerable<PreventivoNCCResponseDTO>> GetPreventiviToExamineAsync(bool isTodo);
    Task<PreventivoNCCResponseDTO> GetPreventivoByIdAsync(long id);
    Task<bool> UpdatePreventivoNCCAsync(long id, PreventivoUpdateNCCDTO preventivo);

}
