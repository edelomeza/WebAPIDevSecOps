using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WebAPIDevSecOps.Context;
using WebAPIDevSecOps.Dto;
using WebAPIDevSecOps.Interfaces;

namespace WebAPIDevSecOps.Services
{
    public class LoginService : ILoginService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IPasswordHasherService _passwordHasher;
        private readonly DbResilienceService _dbResilience;
        private readonly ILogger<LoginService> _logger;
        private readonly IMemoryCache _cache;

        private const string FakeHash = "$argon2id$v=19$m=65536,t=3,p=1$KxY6z3Y9eG7EqJtq98hPqEX7nZaFWoOhiu7z8K7Z4Vwaki3P6KyHRxY6z3Y9eG";
        private const int MaxFailedAttempts = 5;
        private static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(15);
        private static readonly TimeSpan AttemptWindow = TimeSpan.FromMinutes(30);

        public LoginService(AppDbContext context, IConfiguration configuration, IPasswordHasherService passwordHasher, DbResilienceService dbResilience, ILogger<LoginService> logger, IMemoryCache cache)
        {
            _context = context;
            _configuration = configuration;
            _passwordHasher = passwordHasher;
            _dbResilience = dbResilience;
            _logger = logger;
            _cache = cache;
        }

        public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken ct)
        {
            var sw = Stopwatch.StartNew();

            var username = request.User.Trim();

            if (_cache.TryGetValue($"lockout:{username}", out _))
            {
                throw new UnauthorizedAccessException("Cuenta bloqueada temporalmente por múltiples intentos fallidos. Intente de nuevo más tarde.");
            }

            var usuario = await _context.SegUsuario
                .Where(u => u.strNombre == username)
                .FirstOrDefaultAsync(ct);

            sw.Stop();
            _logger.LogInformation("[TIMING] DB query: {ElapsedMs}ms", sw.ElapsedMilliseconds);

            var hash = usuario?.strPWD ?? FakeHash;

            sw.Restart();
            var isValid = await Task.Run(() =>
                _passwordHasher.VerifyPassword(request.Password, hash), ct);
            sw.Stop();
            _logger.LogInformation("[TIMING] VerifyPassword: {ElapsedMs}ms", sw.ElapsedMilliseconds);

            if (usuario == null || !isValid)
            {
                RecordFailedAttempt(username);
                throw new UnauthorizedAccessException("Credenciales inválidas.");
            }

            _cache.Remove($"lockout:{username}");
            _cache.Remove($"attempts:{username}");

            if (await Task.Run(() => _passwordHasher.NeedsRehash(usuario.strPWD), ct))
            {
                sw.Restart();
                usuario.strPWD = await Task.Run(() =>
                    _passwordHasher.HashPassword(request.Password), ct);
                await _dbResilience.SaveChangesAsync(_context, ct);
                sw.Stop();
                _logger.LogInformation("[TIMING] Rehash + Save: {ElapsedMs}ms", sw.ElapsedMilliseconds);
            }

            var token = GenerateJwtToken(usuario.strNombre);

            return new LoginResponse { Token = token };
        }

        private void RecordFailedAttempt(string username)
        {
            var attemptsKey = $"attempts:{username}";
            var attempts = 1;

            if (_cache.TryGetValue(attemptsKey, out int currentAttempts))
            {
                attempts = currentAttempts + 1;
            }

            if (attempts >= MaxFailedAttempts)
            {
                _cache.Set($"lockout:{username}", true, LockoutDuration);
                _cache.Remove(attemptsKey);
            }
            else
            {
                _cache.Set(attemptsKey, attempts, AttemptWindow);
            }
        }

        private string GenerateJwtToken(string username)
        {
            var secretKey = _configuration["Jwt:Key"]
                ?? throw new InvalidOperationException("JWT Key no configurada");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Role, "Admin")
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: DateTime.UtcNow.AddMinutes(60),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
