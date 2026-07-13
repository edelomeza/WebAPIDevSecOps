using Microsoft.EntityFrameworkCore;
using WebAPIDevSecOps.Context;
using WebAPIDevSecOps.Dto;
using WebAPIDevSecOps.Interfaces;
using WebAPIDevSecOps.Models;

namespace WebAPIDevSecOps.Services
{
    public class ProductoService : IProductoService
    {
        private readonly AppDbContext _context;
        private readonly DbResilienceService _dbResilience;

        public ProductoService(AppDbContext context, DbResilienceService dbResilience)
        {
            _context = context;
            _dbResilience = dbResilience;
        }

        public async Task<PagedResult<ProProductoDto>> GetAllAsync(QueryParams? queryParams = null)
        {
            var p = queryParams ?? new QueryParams();

            var query = _context.ProProducto
                .AsNoTracking()
                .Select(x => new ProProductoDto
                {
                    id = x.id,
                    strNombreProducto = x.strNombreProducto,
                    strURLImagen = x.strURLImagen,
                    strDescripcion = x.strDescripcion,
                    intNumeroExistencia = x.intNumeroExistencia,
                    decPrecio = x.decPrecio,
                    RowVersion = x.RowVersion,
                });

            var totalCount = await query.CountAsync();
            query = query.ApplyPagination(p);
            var items = await query.ToListAsync();

            return new PagedResult<ProProductoDto>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = p.PageNumber,
                PageSize = p.PageSize,
            };
        }

        public async Task<PagedResult<ProProductoDto>> SearchByNameAsync(string texto, QueryParams? queryParams = null)
        {
            var p = queryParams ?? new QueryParams();

            var query = _context.ProProducto
                .AsNoTracking()
                .Where(x => x.strNombreProducto.ToLower().Contains(texto.ToLower()))
                .Select(x => new ProProductoDto
                {
                    id = x.id,
                    strNombreProducto = x.strNombreProducto,
                    strURLImagen = x.strURLImagen,
                    strDescripcion = x.strDescripcion,
                    intNumeroExistencia = x.intNumeroExistencia,
                    decPrecio = x.decPrecio,
                    RowVersion = x.RowVersion,
                });

            var totalCount = await query.CountAsync();
            query = query.ApplyPagination(p);
            var items = await query.ToListAsync();

            return new PagedResult<ProProductoDto>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = p.PageNumber,
                PageSize = p.PageSize,
            };
        }

        public async Task<ProProductoDto?> GetByIdAsync(int id)
        {
            return await _context.ProProducto
                .AsNoTracking()
                .Where(x => x.id == id)
                .Select(x => new ProProductoDto
                {
                    id = x.id,
                    strNombreProducto = x.strNombreProducto,
                    strURLImagen = x.strURLImagen,
                    strDescripcion = x.strDescripcion,
                    intNumeroExistencia = x.intNumeroExistencia,
                    decPrecio = x.decPrecio,
                    RowVersion = x.RowVersion,
                })
                .FirstOrDefaultAsync();
        }

        public async Task<ProProductoDto> CreateAsync(ProductoCreateDto dto)
        {
            var producto = new ProProducto
            {
                strNombreProducto = dto.strNombreProducto.Trim(),
                strURLImagen = dto.strURLImagen?.Trim(),
                strDescripcion = dto.strDescripcion?.Trim(),
                intNumeroExistencia = dto.intNumeroExistencia,
                decPrecio = dto.decPrecio,
            };

            _context.ProProducto.Add(producto);
            await _dbResilience.SaveChangesAsync(_context);

            return new ProProductoDto
            {
                id = producto.id,
                strNombreProducto = producto.strNombreProducto,
                strURLImagen = producto.strURLImagen,
                strDescripcion = producto.strDescripcion,
                intNumeroExistencia = producto.intNumeroExistencia,
                decPrecio = producto.decPrecio,
                RowVersion = producto.RowVersion,
            };
        }

        public async Task UpdateAsync(int id, ProductoUpdateDto dto)
        {
            if (id != dto.id)
            {
                throw new ArgumentException("El ID del producto no coincide.");
            }

            var producto = await _context.ProProducto
                .FirstOrDefaultAsync(x => x.id == id);

            if (producto == null)
            {
                throw new KeyNotFoundException("Producto no encontrado.");
            }

            if (dto.RowVersion is { Length: > 0 })
            {
                _context.Entry(producto).Property("RowVersion").OriginalValue = dto.RowVersion;
            }

            producto.strNombreProducto = dto.strNombreProducto.Trim();
            producto.strURLImagen = dto.strURLImagen?.Trim();
            producto.strDescripcion = dto.strDescripcion?.Trim();
            producto.intNumeroExistencia = dto.intNumeroExistencia;
            producto.decPrecio = dto.decPrecio;

            _context.Entry(producto).State = EntityState.Modified;
            await _dbResilience.SaveChangesAsync(_context);
        }

        public async Task DeleteAsync(int id, ProductoDeleteDto dto)
        {
            var producto = await _context.ProProducto
                .FirstOrDefaultAsync(x => x.id == id);

            if (producto == null)
            {
                throw new KeyNotFoundException("Producto no encontrado.");
            }

            if (dto.RowVersion is { Length: > 0 })
            {
                _context.Entry(producto).Property("RowVersion").OriginalValue = dto.RowVersion;
            }

            _context.ProProducto.Remove(producto);

            await _dbResilience.SaveChangesAsync(_context);
        }
    }
}
