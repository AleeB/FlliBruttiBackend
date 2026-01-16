using FlliBrutti.Backend.Application.Responses;

namespace FlliBrutti.Backend.Application.IServices
{
    public interface IFirmaService
    {
        Task<(bool, string)> CreateFirma(long idUser);
        Task<(bool, string)> ExitFirma(long idFirma);
        Task<IEnumerable<FirmaResponseDTO>> GetFirmaByIdUserAsync(long idUser);
    }
}
