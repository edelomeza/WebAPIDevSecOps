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

        public DbSet<EmpCatTipoEmpleado> EmpCatTipoEmpleado { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SegUsuario>()
                .HasIndex(u => u.strNombre)
                .IsUnique();

            modelBuilder.Entity<EmpEmpleado>()
                .HasIndex(e => e.strCURP)
                .IsUnique()
                .HasFilter("[strCURP] IS NOT NULL");

            modelBuilder.Entity<EmpEmpleado>()
                .HasOne(e => e.EmpCatTipoEmpleado)
                .WithMany()
                .HasForeignKey(e => e.idEmpCatTipoEmpleado)
                .OnDelete(DeleteBehavior.SetNull);
        }

        public DbSet<SegUsuario> SegUsuario { get; set; } = default!;
        public DbSet<EmpEmpleado> EmpEmpleado { get; set; } = default!;
        public DbSet<SegTokenBlacklist> SegTokenBlacklist { get; set; } = default!;
    }
}
