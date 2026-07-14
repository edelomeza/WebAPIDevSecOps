using WebAPIDevSecOps.Dto;

namespace WebAPIDevSecOps.Interfaces
{
    public interface IVentaService
    {
        Task<PagedResult<VenVentaDto>> GetAllAsync(QueryParams? queryParams = null);
        Task<VenVentaDto?> GetByIdAsync(int id);
        Task<PagedResult<VenVentaDto>> SearchAsync(string? strClaveVenta = null, string? strNombreCliente = null, DateTime? dteFechaInicio = null, DateTime? dteFechaFin = null, QueryParams? queryParams = null);
        Task<VenVentaDto> CreateAsync(VenVentaCreateDto dto);
    }
}
