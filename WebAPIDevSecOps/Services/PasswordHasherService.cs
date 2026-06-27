using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;
using WebAPIDevSecOps.Interfaces;
using WebAPIDevSecOps.Dto;
using Konscious.Security.Cryptography;

namespace WebAPIDevSecOps.Services
{
    public class PasswordHasherService: IPasswordHasherService
    {
        private readonly PasswordHasherOptions _options;

        public PasswordHasherService(IOptions<PasswordHasherOptions> options)
        {
            _options = options.Value;
        }

        public string HashPassword(string password)
        {
            var salt = RandomNumberGenerator.GetBytes(_options.SaltSize);
            var hash = GetHash(password, salt, _options.MemorySize, _options.Iterations, _options.DegreeOfParallelism);

            var saltEncoded = Convert.ToBase64String(salt).TrimEnd('=');
            var hashEncoded = Convert.ToBase64String(hash).TrimEnd('=');

            return $"$argon2id$v=19$m={_options.MemorySize},t={_options.Iterations},p={_options.DegreeOfParallelism}${saltEncoded}${hashEncoded}";
        }

        public bool VerifyPassword(string password, string hashedPassword)
        {
            if (string.IsNullOrWhiteSpace(hashedPassword))
                return false;

            if (hashedPassword.StartsWith("$argon2id$", StringComparison.Ordinal))
                return VerifyArgon2id(password, hashedPassword);

            if (hashedPassword.StartsWith("$2a$", StringComparison.Ordinal) ||
                hashedPassword.StartsWith("$2b$", StringComparison.Ordinal))
                return BCrypt.Net.BCrypt.Verify(password, hashedPassword);

            return false;
        }

        public bool NeedsRehash(string hashedPassword)
        {
            if (string.IsNullOrWhiteSpace(hashedPassword))
                return true;

            if (hashedPassword.StartsWith("$2a$", StringComparison.Ordinal) ||
                hashedPassword.StartsWith("$2b$", StringComparison.Ordinal))
                return true;

            if (hashedPassword.StartsWith("$argon2id$", StringComparison.Ordinal))
            {
                var parts = hashedPassword.Split('$');
                if (parts.Length < 5)
                    return true;

                var parameters = parts[3];
                var expected = $"m={_options.MemorySize},t={_options.Iterations},p={_options.DegreeOfParallelism}";
                return parameters != expected;
            }

            return true;
        }

        private static byte[] GetHash(string password, byte[] salt, int memorySize, int iterations, int parallelism)
        {
            using var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password))
            {
                Salt = salt,
                MemorySize = memorySize,
                Iterations = iterations,
                DegreeOfParallelism = parallelism
            };
            return argon2.GetBytes(32);
        }

        private static bool VerifyArgon2id(string password, string hashedPassword)
        {
            try
            {
                var parts = hashedPassword.Split('$');
                if (parts.Length < 6)
                    return false;

                var parameters = parts[3];
                var saltEncoded = parts[4];
                var hashEncoded = parts[5];

                var salt = Convert.FromBase64String(PadBase64(saltEncoded));
                var expectedHash = Convert.FromBase64String(PadBase64(hashEncoded));

                var paramParts = parameters.Split(',');
                if (paramParts.Length < 3)
                    return false;

                var memorySize = int.Parse(paramParts[0].Split('=')[1]);
                var iterations = int.Parse(paramParts[1].Split('=')[1]);
                var parallelism = int.Parse(paramParts[2].Split('=')[1]);

                var hash = GetHash(password, salt, memorySize, iterations, parallelism);

                return CryptographicOperations.FixedTimeEquals(expectedHash, hash);
            }
            catch
            {
                return false;
            }
        }

        private static string PadBase64(string base64)
        {
            var remainder = base64.Length % 4;
            if (remainder > 0)
                base64 += new string('=', 4 - remainder);
            return base64;
        }
    }
}
