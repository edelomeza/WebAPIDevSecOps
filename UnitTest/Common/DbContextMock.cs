using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebAPIDevSecOps.Context;

namespace UnitTest.Common
{
    public static class DbContextMock
    {
        public static AppDbContext GetDbContext()
        {
            
            var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .EnableSensitiveDataLogging()
            .Options;

            var dbContext = new TestDbContext(options);
            dbContext.Database.EnsureCreated();
            return dbContext;
        }
    }
}