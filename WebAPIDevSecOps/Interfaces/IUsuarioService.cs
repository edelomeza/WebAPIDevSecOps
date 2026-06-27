using WebAPIDevSecOps.Dto;
using WebAPIDevSecOps.Models;

namespace WebAPIDevSecOps.Interfaces
{
    public interface IUsuarioService
    {
        Task<PagedResult<SegUsuarioDto>> GetAllAsync(QueryParams? queryParams = null);
        Task<SegUsuarioDto?> GetByIdAsync(int id);
        Task<SegUsuarioDto> CreateAsync(UsuarioCreateDto dto);
        Task UpdateAsync(int id, UsuarioUpdateDto dto);
        Task DeleteAsync(int id, UsuarioDeleteDto dto);
    }
}
