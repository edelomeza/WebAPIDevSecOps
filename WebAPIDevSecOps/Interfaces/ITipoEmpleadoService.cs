using WebAPIDevSecOps.Dto;

namespace WebAPIDevSecOps.Interfaces
{
    public interface ITipoEmpleadoService
    {
        Task<PagedResult<EmpCatTipoEmpleadoDto>> GetAllAsync(QueryParams? queryParams = null);
        Task<EmpCatTipoEmpleadoDto?> GetByIdAsync(int id);
    }
}
