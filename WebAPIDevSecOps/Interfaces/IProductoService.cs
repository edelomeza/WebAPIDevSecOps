using WebAPIDevSecOps.Dto;
using WebAPIDevSecOps.Models;

namespace WebAPIDevSecOps.Interfaces
{
    public interface IProductoService
    {
        Task<PagedResult<ProProductoDto>> GetAllAsync(QueryParams? queryParams = null);
        Task<PagedResult<ProProductoDto>> SearchByNameAsync(string texto, QueryParams? queryParams = null);
        Task<ProProductoDto?> GetByIdAsync(int id);
        Task<ProProductoDto> CreateAsync(ProductoCreateDto dto);
        Task UpdateAsync(int id, ProductoUpdateDto dto);
        Task DeleteAsync(int id, ProductoDeleteDto dto);
    }
}
