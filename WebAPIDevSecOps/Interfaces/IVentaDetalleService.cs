using WebAPIDevSecOps.Dto;

namespace WebAPIDevSecOps.Interfaces
{
    public interface IVentaDetalleService
    {
        Task<PagedResult<VenVentaDetalleDto>> GetAllAsync(QueryParams? queryParams = null);
        Task<VenVentaDetalleDto?> GetByIdAsync(int id);
        Task<IEnumerable<ProProductoAutocompleteDto>> BuscarProductoAsync(string texto, int maxResultados = 10);
        Task<VenVentaDetalleDto> CreateAsync(VenVentaDetalleCreateDto dto);
    }
}
