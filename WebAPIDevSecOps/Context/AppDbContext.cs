using Microsoft.EntityFrameworkCore;
using WebAPIDevSecOps.Models;

namespace WebAPIDevSecOps.Context
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext>
           options) : base(options)
        {
        }

        public DbSet<Models.EmpCatTipoEmpleado> EmpCatTipoEmpleado { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
           modelBuilder.Entity<SegUsuario>()
                .HasIndex(u => u.strNombre)
                .IsUnique();
        }
        public DbSet<SegUsuario> SegUsuario { get; set; } = default!;
    }
}
