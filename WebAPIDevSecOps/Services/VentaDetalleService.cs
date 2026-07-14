using Microsoft.EntityFrameworkCore;
using WebAPIDevSecOps.Context;
using WebAPIDevSecOps.Dto;
using WebAPIDevSecOps.Interfaces;
using WebAPIDevSecOps.Models;

namespace WebAPIDevSecOps.Services
{
    public class VentaDetalleService : IVentaDetalleService
    {
        private readonly AppDbContext _context;
        private readonly DbResilienceService _dbResilience;

        public VentaDetalleService(AppDbContext context, DbResilienceService dbResilience)
        {
            _context = context;
            _dbResilience = dbResilience;
        }

        public async Task<PagedResult<VenVentaDetalleDto>> GetAllAsync(QueryParams? queryParams = null)
        {
            var p = queryParams ?? new QueryParams();

            var query = _context.Set<VenVentaDetalle>()
                .AsNoTracking()
                .Include(vd => vd.ProProducto)
                .Select(vd => new VenVentaDetalleDto
                {
                    id = vd.id,
                    idVenVenta = vd.idVenVenta,
                    idProProducto = vd.idProProducto,
                    strNombreProducto = vd.ProProducto != null ? vd.ProProducto.strNombreProducto : null,
                    decPrecio = vd.ProProducto != null ? vd.ProProducto.decPrecio : 0,
                    intPiezaVenta = vd.intPiezaVenta,
                    decTotalVenta = vd.decTotalVenta,
                    RowVersion = vd.RowVersion,
                });

            var totalCount = await query.CountAsync();
            query = query.ApplyPagination(p);
            var items = await query.ToListAsync();

            return new PagedResult<VenVentaDetalleDto>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = p.PageNumber,
                PageSize = p.PageSize,
            };
        }

        public async Task<VenVentaDetalleDto?> GetByIdAsync(int id)
        {
            return await _context.Set<VenVentaDetalle>()
                .AsNoTracking()
                .Include(vd => vd.ProProducto)
                .Where(vd => vd.id == id)
                .Select(vd => new VenVentaDetalleDto
                {
                    id = vd.id,
                    idVenVenta = vd.idVenVenta,
                    idProProducto = vd.idProProducto,
                    strNombreProducto = vd.ProProducto != null ? vd.ProProducto.strNombreProducto : null,
                    decPrecio = vd.ProProducto != null ? vd.ProProducto.decPrecio : 0,
                    intPiezaVenta = vd.intPiezaVenta,
                    decTotalVenta = vd.decTotalVenta,
                    RowVersion = vd.RowVersion,
                })
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<ProProductoAutocompleteDto>> BuscarProductoAsync(string texto, int maxResultados = 10)
        {
            return await _context.Set<ProProducto>()
                .AsNoTracking()
                .Where(p => p.strNombreProducto.ToLower().Contains(texto.ToLower()))
                .OrderBy(p => p.strNombreProducto)
                .Take(maxResultados)
                .Select(p => new ProProductoAutocompleteDto
                {
                    id = p.id,
                    strTextoAutocomplete = $"{p.strNombreProducto} | #: {p.intNumeroExistencia} | $: {p.decPrecio}"
                })
                .ToListAsync();
        }

        public async Task<VenVentaDetalleDto> CreateAsync(VenVentaDetalleCreateDto dto)
        {
            var ventaExiste = await _context.Set<VenVenta>().AnyAsync(v => v.id == dto.idVenVenta);
            if (!ventaExiste)
            {
                throw new ArgumentException("La venta especificada no existe.");
            }

            var producto = await _context.Set<ProProducto>()
                .Where(p => p.id == dto.idProProducto)
                .Select(p => new { p.decPrecio })
                .FirstOrDefaultAsync();

            if (producto == null)
            {
                throw new ArgumentException("El producto especificado no existe.");
            }

            var detalle = new VenVentaDetalle
            {
                idVenVenta = dto.idVenVenta,
                idProProducto = dto.idProProducto,
                intPiezaVenta = dto.intPiezaVenta,
                decTotalVenta = dto.intPiezaVenta * producto.decPrecio,
            };

            _context.Set<VenVentaDetalle>().Add(detalle);
            await _dbResilience.SaveChangesAsync(_context);

            return (await GetByIdAsync(detalle.id))!;
        }
    }
}
