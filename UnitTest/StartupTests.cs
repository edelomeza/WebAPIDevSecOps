using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebAPIDevSecOps;

namespace UnitTest
{
    public class StartupTests
    {
        [Fact]
        public async Task Should_Throw_When_JwtKey_Missing()
        {
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            {
                var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
                {
                    builder.UseSetting("Jwt:Key", "");
                    builder.UseSetting("Jwt:Issuer", "edelmeza.com");
                    builder.UseSetting("Jwt:Audience", "edelmeza.com");
                    builder.UseSetting("UseInMemoryDatabase", "true");
                    builder.UseSetting("ConnectionStrings:DefaultConnection", "Server=.;Database=Test;Trusted_Connection=True;");
                });
                return Task.FromResult(factory.CreateClient());
            });

            Assert.Contains("JWT Key", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task Should_Throw_When_JwtKey_TooShort()
        {
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            {
                var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
                {
                    builder.UseSetting("Jwt:Key", "short");
                    builder.UseSetting("Jwt:Issuer", "edelmeza.com");
                    builder.UseSetting("Jwt:Audience", "edelmeza.com");
                    builder.UseSetting("UseInMemoryDatabase", "true");
                    builder.UseSetting("ConnectionStrings:DefaultConnection", "Server=.;Database=Test;Trusted_Connection=True;");
                });
                return Task.FromResult(factory.CreateClient());
            });

            Assert.Contains("32 bytes", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task Should_Throw_When_ConnectionString_Missing()
        {
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            {
                var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
                {
                    builder.UseSetting("Jwt:Key", "01123581321345589144233377610987");
                    builder.UseSetting("Jwt:Issuer", "edelmeza.com");
                    builder.UseSetting("Jwt:Audience", "edelmeza.com");
                    builder.UseSetting("UseInMemoryDatabase", "false");
                    builder.UseSetting("ConnectionStrings:DefaultConnection", "");
                });
                return Task.FromResult(factory.CreateClient());
            });

            Assert.Contains("DefaultConnection", ex.Message, StringComparison.OrdinalIgnoreCase);
        }
    }
}
