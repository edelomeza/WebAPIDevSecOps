using WebAPIDevSecOps.Dto;
using WebAPIDevSecOps.Models;

namespace WebAPIDevSecOps.Interfaces
{
    public interface IClienteService
    {
        Task<PagedResult<CliClienteDto>> GetAllAsync(QueryParams? queryParams = null);
        Task<PagedResult<CliClienteDto>> SearchByNameAsync(string texto, QueryParams? queryParams = null);
        Task<CliClienteDto?> GetByIdAsync(int id);
        Task<CliClienteDto> CreateAsync(CliClienteCreateDto dto);
        Task UpdateAsync(int id, CliClienteUpdateDto dto);
        Task DeleteAsync(int id, CliClienteDeleteDto dto);
    }
}
