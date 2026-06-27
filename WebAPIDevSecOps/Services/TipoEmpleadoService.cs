using Microsoft.EntityFrameworkCore;
using WebAPIDevSecOps.Context;
using WebAPIDevSecOps.Dto;
using WebAPIDevSecOps.Interfaces;

namespace WebAPIDevSecOps.Services
{
    public class TipoEmpleadoService : ITipoEmpleadoService
    {
        private readonly AppDbContext _context;

        public TipoEmpleadoService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<EmpCatTipoEmpleadoDto>> GetAllAsync(QueryParams? queryParams = null)
        {
            var p = queryParams ?? new QueryParams();

            var query = _context.EmpCatTipoEmpleado
                .AsNoTracking()
                .Select(t => new EmpCatTipoEmpleadoDto
                {
                    id = t.id,
                    strValor = t.strValor,
                    strDescripcion = t.strDescripcion
                });

            var totalCount = await query.CountAsync();

            query = query.ApplyPagination(p);

            var items = await query.ToListAsync();

            return new PagedResult<EmpCatTipoEmpleadoDto>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = p.PageNumber,
                PageSize = p.PageSize
            };
        }

        public async Task<EmpCatTipoEmpleadoDto?> GetByIdAsync(int id)
        {
            return await _context.EmpCatTipoEmpleado
                .AsNoTracking()
                .Where(t => t.id == id)
                .Select(t => new EmpCatTipoEmpleadoDto
                {
                    id = t.id,
                    strValor = t.strValor,
                    strDescripcion = t.strDescripcion
                })
                .FirstOrDefaultAsync();
        }

    }
}
