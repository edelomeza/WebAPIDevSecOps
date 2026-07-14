using WebAPIDevSecOps.Dto;

namespace WebAPIDevSecOps.Interfaces
{
    public interface IVenCatEstadoService
    {
        Task<PagedResult<VenCatEstadoDto>> GetAllAsync(QueryParams? queryParams = null);
        Task<VenCatEstadoDto?> GetByIdAsync(int id);
    }
}
