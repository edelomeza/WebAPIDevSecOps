using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace UnitTest
{
    public class ConfigTests
    {
        [Fact]
        public void Should_Override_ConnectionString_With_DB_USER_And_DB_PASSWORD()
        {
            var baseConnectionString = "Server=188.40.211.8; Database=db45497; Encrypt=True; MultipleActiveResultSets=True; TrustServerCertificate=True; Min Pool Size=2; Max Pool Size=200; Connection Lifetime=300;";

            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:DefaultConnection"] = baseConnectionString,
                    ["DB_USER"] = "test_user",
                    ["DB_PASSWORD"] = "test_pass"
                })
                .Build();

            var connectionString = config.GetConnectionString("DefaultConnection");
            var dbUser = config["DB_USER"];
            var dbPassword = config["DB_PASSWORD"];

            var connBuilder = new SqlConnectionStringBuilder(connectionString)
            {
                UserID = dbUser,
                Password = dbPassword
            };
            var result = connBuilder.ConnectionString;

            Assert.Contains("User ID=test_user", result, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("Password=test_pass", result, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("Data Source=188.40.211.8", result, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("Initial Catalog=db45497", result, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void Should_Use_Original_ConnectionString_When_No_DB_User()
        {
            var baseConnectionString = "Server=188.40.211.8; Database=db45497; Encrypt=True; MultipleActiveResultSets=True; TrustServerCertificate=True; Min Pool Size=2; Max Pool Size=200; Connection Lifetime=300;";

            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:DefaultConnection"] = baseConnectionString
                })
                .Build();

            var connectionString = config.GetConnectionString("DefaultConnection");
            var dbUser = config["DB_USER"];
            var dbPassword = config["DB_PASSWORD"];

            if (!string.IsNullOrEmpty(dbUser) && !string.IsNullOrEmpty(dbPassword))
            {
                var connBuilder = new SqlConnectionStringBuilder(connectionString)
                {
                    UserID = dbUser,
                    Password = dbPassword
                };
                connectionString = connBuilder.ConnectionString;
            }

            Assert.Equal(baseConnectionString, connectionString);
            Assert.Null(dbUser);
            Assert.Null(dbPassword);
        }

        [Fact]
        public void CORS_Should_Read_From_Config_Or_Fallback()
        {
            var configWithOrigin = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Cors:AllowedOrigin"] = "https://myapp.ondigitalocean.app"
                })
                .Build();

            var origin1 = configWithOrigin["Cors:AllowedOrigin"]
                ?? Environment.GetEnvironmentVariable("CORS_ALLOWED_ORIGIN")
                ?? "https://localhost:5097";

            Assert.Equal("https://myapp.ondigitalocean.app", origin1);

            var configWithoutOrigin = new ConfigurationBuilder()
                .Build();

            var origin2 = configWithoutOrigin["Cors:AllowedOrigin"]
                ?? Environment.GetEnvironmentVariable("CORS_ALLOWED_ORIGIN")
                ?? "https://localhost:5097";

            Assert.Equal("https://localhost:5097", origin2);
        }
    }
}
