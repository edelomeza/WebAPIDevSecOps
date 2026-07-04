using WebAPIDevSecOps.Dto;

namespace WebAPIDevSecOps.Interfaces
{
    public interface IEmpleadoService
    {
        Task<PagedResult<EmpEmpleadoDto>> GetAllAsync(QueryParams? queryParams = null);
        Task<EmpEmpleadoDto?> GetByIdAsync(int id);
        Task<PagedResult<EmpEmpleadoDto>> SearchAsync(string? texto, int? idTipoEmpleado, QueryParams? queryParams = null);
        Task<EmpEmpleadoDto> CreateAsync(EmpEmpleadoCreateDto dto);
        Task UpdateAsync(int id, EmpEmpleadoUpdateDto dto);
        Task DeleteAsync(int id, EmpEmpleadoDeleteDto dto);
    }
}
