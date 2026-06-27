using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebAPIDevSecOps.Context;
using WebAPIDevSecOps.Models;

namespace UnitTest.Common
{
    public class TestDbContext : AppDbContext
    {
        public TestDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            foreach (var entry in ChangeTracker.Entries<SegUsuario>())
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.RowVersion ??= new byte[] { 1, 0, 0, 0 };
                }
            }

            return base.SaveChangesAsync(cancellationToken);
        }
    }
}
