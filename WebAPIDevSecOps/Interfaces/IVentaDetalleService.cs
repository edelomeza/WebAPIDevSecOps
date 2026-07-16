using WebAPIDevSecOps.Dto;

namespace WebAPIDevSecOps.Interfaces
{
public interface IVentaDetalleService
{
    Task<PagedResult<VenVentaDetalleDto>> GetAllAsync(QueryParams? queryParams = null);
    Task<VenVentaDetalleDto?> GetByIdAsync(int id);
    Task<IEnumerable<ProProductoAutocompleteDto>> AutocompleteProductoAsync(string texto, int maxResultados = 10);
    Task<VenVentaDetalleDto> CreateAsync(VenVentaDetalleCreateDto dto);
    Task UpdateAsync(int id, VenVentaDetalleUpdateDto dto);
    Task DeleteAsync(int id, VenVentaDetalleDeleteDto dto);
}
}
