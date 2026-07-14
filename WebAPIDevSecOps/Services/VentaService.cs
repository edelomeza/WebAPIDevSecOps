using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using WebAPIDevSecOps.Context;
using WebAPIDevSecOps.Dto;
using WebAPIDevSecOps.Interfaces;
using WebAPIDevSecOps.Models;

namespace WebAPIDevSecOps.Services
{
    public class VentaService : IVentaService
    {
        private readonly AppDbContext _context;
        private readonly DbResilienceService _dbResilience;

        public VentaService(AppDbContext context, DbResilienceService dbResilience)
        {
            _context = context;
            _dbResilience = dbResilience;
        }

        public async Task<PagedResult<VenVentaDto>> GetAllAsync(QueryParams? queryParams = null)
        {
            var p = queryParams ?? new QueryParams();

            var query = _context.Set<VenVenta>()
                .AsNoTracking()
                .Include(v => v.CliCliente)
                .Include(v => v.SegUsuario)
                .Include(v => v.VenCatEstado)
                .Select(v => new VenVentaDto
                {
                    id = v.id,
                    idCliCliente = v.idCliCliente,
                    strNombreCliente = v.CliCliente != null ? v.CliCliente.strNombreCliente : null,
                    idSegUsuario = v.idSegUsuario,
                    strNombreUsuario = v.SegUsuario != null ? v.SegUsuario.strNombre : null,
                    idVenCatEstado = v.idVenCatEstado,
                    strEstado = v.VenCatEstado != null ? v.VenCatEstado.strValor : null,
                    dteFechaHoraCompra = v.dteFechaHoraCompra,
                    strClaveVenta = v.strClaveVenta,
                    RowVersion = v.RowVersion,
                });

            var totalCount = await query.CountAsync();
            query = query.ApplyPagination(p);
            var items = await query.ToListAsync();

            return new PagedResult<VenVentaDto>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = p.PageNumber,
                PageSize = p.PageSize,
            };
        }

        public async Task<VenVentaDto?> GetByIdAsync(int id)
        {
            return await _context.Set<VenVenta>()
                .AsNoTracking()
                .Include(v => v.CliCliente)
                .Include(v => v.SegUsuario)
                .Include(v => v.VenCatEstado)
                .Where(v => v.id == id)
                .Select(v => new VenVentaDto
                {
                    id = v.id,
                    idCliCliente = v.idCliCliente,
                    strNombreCliente = v.CliCliente != null ? v.CliCliente.strNombreCliente : null,
                    idSegUsuario = v.idSegUsuario,
                    strNombreUsuario = v.SegUsuario != null ? v.SegUsuario.strNombre : null,
                    idVenCatEstado = v.idVenCatEstado,
                    strEstado = v.VenCatEstado != null ? v.VenCatEstado.strValor : null,
                    dteFechaHoraCompra = v.dteFechaHoraCompra,
                    strClaveVenta = v.strClaveVenta,
                    RowVersion = v.RowVersion,
                })
                .FirstOrDefaultAsync();
        }

        public async Task<PagedResult<VenVentaDto>> SearchAsync(string? strClaveVenta, string? strNombreCliente, DateTime? dteFechaInicio, DateTime? dteFechaFin, QueryParams? queryParams = null)
        {
            var p = queryParams ?? new QueryParams();

            var query = _context.Set<VenVenta>()
                .AsNoTracking()
                .Include(v => v.CliCliente)
                .Include(v => v.SegUsuario)
                .Include(v => v.VenCatEstado)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(strClaveVenta))
            {
                query = query.Where(v => v.strClaveVenta.ToLower().Contains(strClaveVenta.ToLower()));
            }

            if (!string.IsNullOrWhiteSpace(strNombreCliente))
            {
                query = query.Where(v => v.CliCliente != null && v.CliCliente.strNombreCliente.ToLower().Contains(strNombreCliente.ToLower()));
            }

            if (dteFechaInicio.HasValue)
            {
                var startDate = dteFechaInicio.Value.Date;
                query = query.Where(v => v.dteFechaHoraCompra >= startDate);
            }

            if (dteFechaFin.HasValue)
            {
                var endDate = dteFechaFin.Value.Date.AddDays(1);
                query = query.Where(v => v.dteFechaHoraCompra < endDate);
            }

            var selectQuery = query.Select(v => new VenVentaDto
            {
                id = v.id,
                idCliCliente = v.idCliCliente,
                strNombreCliente = v.CliCliente != null ? v.CliCliente.strNombreCliente : null,
                idSegUsuario = v.idSegUsuario,
                strNombreUsuario = v.SegUsuario != null ? v.SegUsuario.strNombre : null,
                idVenCatEstado = v.idVenCatEstado,
                strEstado = v.VenCatEstado != null ? v.VenCatEstado.strValor : null,
                dteFechaHoraCompra = v.dteFechaHoraCompra,
                strClaveVenta = v.strClaveVenta,
                RowVersion = v.RowVersion,
            });

            var totalCount = await selectQuery.CountAsync();
            selectQuery = selectQuery.ApplyPagination(p);
            var items = await selectQuery.ToListAsync();

            return new PagedResult<VenVentaDto>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = p.PageNumber,
                PageSize = p.PageSize,
            };
        }

        public async Task<VenVentaDto> CreateAsync(VenVentaCreateDto dto)
        {
            var clienteExiste = await _context.CliCliente.AnyAsync(c => c.id == dto.idCliCliente);
            if (!clienteExiste)
            {
                throw new ArgumentException("El cliente especificado no existe.");
            }

            var usuarioExiste = await _context.SegUsuario.AnyAsync(u => u.id == dto.idSegUsuario);
            if (!usuarioExiste)
            {
                throw new ArgumentException("El usuario especificado no existe.");
            }

            var claveVenta = await GenerarClaveVentaUnicaAsync();

            var venta = new VenVenta
            {
                idCliCliente = dto.idCliCliente,
                idSegUsuario = dto.idSegUsuario,
                idVenCatEstado = 1,
                dteFechaHoraCompra = DateTime.UtcNow,
                strClaveVenta = claveVenta,
            };

            _context.Set<VenVenta>().Add(venta);
            await _dbResilience.SaveChangesAsync(_context);

            return (await GetByIdAsync(venta.id))!;
        }

        private async Task<string> GenerarClaveVentaUnicaAsync()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var maxAttempts = 10;

            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                var data = new byte[10];
                RandomNumberGenerator.Fill(data);
                var clave = new string(data.Select(b => chars[b % chars.Length]).ToArray());
                var existe = await _context.Set<VenVenta>().AnyAsync(v => v.strClaveVenta == clave);
                if (!existe)
                {
                    return clave;
                }
            }

            throw new InvalidOperationException("No se pudo generar una clave de venta única después de varios intentos.");
        }
    }
}
