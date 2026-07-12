using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;
using System.IdentityModel.Tokens.Jwt;
using WebAPIDevSecOps.Context;
using WebAPIDevSecOps.Models;

namespace WebAPIDevSecOps.Services
{
    public class TokenBlacklist
    {
        private static readonly ConcurrentDictionary<string, DateTime> _tokens = new();
        private static readonly JwtSecurityTokenHandler _handler = new();
        private static int _operationCount = 0;
        private static IServiceScopeFactory? _scopeFactory;

        public static void Initialize(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        public static void Add(string token)
        {
            var jwt = TryReadToken(token);
            var jti = jwt?.Id ?? token;
            var expiry = jwt?.ValidTo ?? DateTime.UtcNow.AddMinutes(60);

            _tokens.TryAdd(jti, expiry);

            PersistToDb(jti, expiry);

            if (Interlocked.Increment(ref _operationCount) % 10 == 0)
            {
                CleanupExpired();
            }
        }

        public static bool IsBlacklisted(string token)
        {
            var jwt = TryReadToken(token);
            var jti = jwt?.Id ?? token;

            if (_tokens.TryGetValue(jti, out var expiry))
            {
                if (expiry <= DateTime.UtcNow)
                {
                    _tokens.TryRemove(jti, out _);
                    RemoveFromDb(jti);
                    return false;
                }
                return true;
            }

            return CheckDb(jti);
        }

        public static void CleanupExpired()
        {
            var now = DateTime.UtcNow;
            foreach (var kv in _tokens)
            {
                if (kv.Value <= now)
                {
                    _tokens.TryRemove(kv.Key, out _);
                }
            }

            CleanupDbExpired(now);
        }

        public static void Clear()
        {
            _tokens.Clear();
            ClearDb();
        }

        public static int Count => _tokens.Count;

        private static JwtSecurityToken? TryReadToken(string token)
        {
            try { return _handler.ReadJwtToken(token); }
            catch { return null; }
        }

        private static void PersistToDb(string jti, DateTime expiry)
        {
            if (_scopeFactory == null) return;
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                db.SegTokenBlacklist.Add(new SegTokenBlacklist { Jti = jti, ExpiryUtc = expiry });
                db.SaveChanges();
            }
            catch { }
        }

        private static bool CheckDb(string jti)
        {
            if (_scopeFactory == null) return false;
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var entry = db.SegTokenBlacklist.AsNoTracking().FirstOrDefault(e => e.Jti == jti);
                if (entry != null)
                {
                    if (entry.ExpiryUtc <= DateTime.UtcNow)
                    {
                        RemoveFromDb(jti);
                        return false;
                    }
                    _tokens.TryAdd(entry.Jti, entry.ExpiryUtc);
                    return true;
                }
            }
            catch { }
            return false;
        }

        private static void RemoveFromDb(string jti)
        {
            if (_scopeFactory == null) return;
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var entry = db.SegTokenBlacklist.Find(jti);
                if (entry != null)
                {
                    db.SegTokenBlacklist.Remove(entry);
                    db.SaveChanges();
                }
            }
            catch { }
        }

        private static void CleanupDbExpired(DateTime now)
        {
            if (_scopeFactory == null) return;
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var expired = db.SegTokenBlacklist.Where(e => e.ExpiryUtc <= now).ToList();
                db.SegTokenBlacklist.RemoveRange(expired);
                db.SaveChanges();
            }
            catch { }
        }

        private static void ClearDb()
        {
            if (_scopeFactory == null) return;
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                db.SegTokenBlacklist.RemoveRange(db.SegTokenBlacklist);
                db.SaveChanges();
            }
            catch { }
        }
    }
}
