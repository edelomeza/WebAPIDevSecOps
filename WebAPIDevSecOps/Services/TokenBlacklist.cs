using System.Collections.Concurrent;
using System.IdentityModel.Tokens.Jwt;

namespace WebAPIDevSecOps.Services
{
    public class TokenBlacklist
    {
        private static readonly ConcurrentDictionary<string, DateTime> _tokens = new();
        private static readonly JwtSecurityTokenHandler _handler = new();
        private static int _operationCount = 0;

        public static void Add(string token)
        {
            var jwt = TryReadToken(token);
            var key = jwt?.Id ?? token;
            var expiry = jwt?.ValidTo ?? DateTime.UtcNow.AddMinutes(60);

            _tokens.TryAdd(key, expiry);

            if (Interlocked.Increment(ref _operationCount) % 10 == 0)
            {
                CleanupExpired();
            }
        }

        public static bool IsBlacklisted(string token)
        {
            var jwt = TryReadToken(token);
            var key = jwt?.Id ?? token;

            if (_tokens.TryGetValue(key, out var expiry))
            {
                if (expiry <= DateTime.UtcNow)
                {
                    _tokens.TryRemove(key, out _);
                    return false;
                }
                return true;
            }
            return false;
        }

        private static JwtSecurityToken? TryReadToken(string token)
        {
            try
            {
                return _handler.ReadJwtToken(token);
            }
            catch
            {
                return null;
            }
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
        }

        public static int Count => _tokens.Count;
    }
}
