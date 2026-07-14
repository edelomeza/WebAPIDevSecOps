using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using UnitTest.Common;
using WebAPIDevSecOps.Services;

namespace SecurityTest.Venta;

public class SecurityTests : IClassFixture<WebApplicationFactory<Program>>, IAsyncLifetime
{
    private readonly HttpClient _client;
    private const string JwtKey = "01123581321345589144233377610987";
    private const string JwtIssuer = "edelmeza.com";
    private const string JwtAudience = "edelmeza.com";

    public SecurityTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.WithWebHostBuilder(builder =>
        {
            builder.UseSetting("ConnectionStrings:DefaultConnection", "Server=.;Database=Test;Trusted_Connection=True;");
            builder.UseSetting("Jwt:Key", JwtKey);
            builder.UseSetting("Jwt:Issuer", JwtIssuer);
            builder.UseSetting("Jwt:Audience", JwtAudience);
            builder.UseSetting("UseInMemoryDatabase", "true");
            builder.UseSetting("InMemoryDatabaseName", $"SecurityTestVentaDb_{Guid.NewGuid():N}");
        }).CreateClient();
    }

    public Task InitializeAsync()
    {
        TokenBlacklist.Clear();
        return Task.CompletedTask;
    }

    public Task DisposeAsync() => Task.CompletedTask;

    private string AdminToken => TokenHelper.GenerateValidToken(JwtKey, JwtIssuer, JwtAudience);

    [Fact]
    public async Task Should_Reject_Request_Without_Token()
    {
        var getResponse = await _client.GetAsync("/api/v1/venta");
        Assert.Equal(HttpStatusCode.Unauthorized, getResponse.StatusCode);

        var getByIdResponse = await _client.GetAsync("/api/v1/venta/1");
        Assert.Equal(HttpStatusCode.Unauthorized, getByIdResponse.StatusCode);

        var postResponse = await _client.PostAsJsonAsync("/api/v1/venta", new { });
        Assert.Equal(HttpStatusCode.Unauthorized, postResponse.StatusCode);
    }

    [Fact]
    public async Task Should_Reject_Token_With_Wrong_Role()
    {
        var userToken = TokenHelper.GenerateTokenWithRole(JwtKey, JwtIssuer, JwtAudience, "User");

        var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/venta");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", userToken);

        var response = await _client.SendAsync(request);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Should_Reject_Expired_Token()
    {
        var expiredToken = TokenHelper.GenerateExpiredToken(JwtKey, JwtIssuer, JwtAudience);

        var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/venta");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", expiredToken);

        var response = await _client.SendAsync(request);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Should_Reject_Blacklisted_Token()
    {
        var token = TokenHelper.GenerateValidToken(JwtKey, JwtIssuer, JwtAudience);
        TokenBlacklist.Add(token);

        var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/venta");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.SendAsync(request);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Should_Reject_Tampered_Token()
    {
        var token = TokenHelper.GenerateValidToken(JwtKey, JwtIssuer, JwtAudience);
        var tamperedToken = token[..^2] + "xx";

        var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/venta");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tamperedToken);

        var response = await _client.SendAsync(request);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Should_Reject_Invalid_Signature()
    {
        var fakeKey = "clave_falsa_super_insegura_123456789";
        var fakeToken = TokenHelper.GenerateValidToken(fakeKey, JwtIssuer, JwtAudience);

        var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/venta");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", fakeToken);

        var response = await _client.SendAsync(request);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Should_Reject_Negative_Id()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/venta/-1");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", AdminToken);

        var response = await _client.SendAsync(request);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Should_Reject_NonExistent_Venta()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/venta/9999");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", AdminToken);

        var response = await _client.SendAsync(request);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Should_Reject_Create_With_Empty_IdCliCliente()
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/venta")
        {
            Content = JsonContent.Create(new { idCliCliente = 0, idSegUsuario = 1 })
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", AdminToken);

        var response = await _client.SendAsync(request);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Should_Reject_Create_With_Empty_IdSegUsuario()
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/venta")
        {
            Content = JsonContent.Create(new { idCliCliente = 1, idSegUsuario = 0 })
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", AdminToken);

        var response = await _client.SendAsync(request);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Should_Contain_Security_Headers()
    {
        var response = await _client.GetAsync("/api/v1/venta");

        Assert.True(response.Headers.Contains("X-Content-Type-Options"));
        Assert.True(response.Headers.Contains("X-Frame-Options"));
        Assert.True(response.Headers.Contains("X-XSS-Protection"));
        Assert.True(response.Headers.Contains("Strict-Transport-Security"));
    }

    [Fact]
    public async Task Should_Not_Cache_Authenticated_Responses()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/venta");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", AdminToken);

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("no-store", response.Headers.CacheControl?.ToString() ?? "");
    }
}
