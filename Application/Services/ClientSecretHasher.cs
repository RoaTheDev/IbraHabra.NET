using System.Security.Cryptography;
using System.Text;
using IbraHabra.NET.Domain.Contract.Services;
using Isopoh.Cryptography.Argon2;
using Microsoft.Extensions.Options;

namespace IbraHabra.NET.Application.Services
{
    public class Argon2Options
    {
        public int MemoryCostMb { get; set; } = 128;
        public int TimeCost { get; set; } = 4;
        public int Parallelism { get; set; } = 4;
        public int SaltLength { get; set; } = 16;
    }

    public class ClientSecretHasher : IClientSecretHasher
    {
        private readonly Argon2Options _options;

        public ClientSecretHasher(IOptions<Argon2Options> options)
        {
            _options = options.Value;
        }

        public string HashSecret(string plainTextSecret)
        {
            if (string.IsNullOrWhiteSpace(plainTextSecret))
                throw new ArgumentException("Secret cannot be null or empty.", nameof(plainTextSecret));

            byte[] salt = new byte[_options.SaltLength];
            RandomNumberGenerator.Fill(salt);

            var config = new Argon2Config
            {
                Type = Argon2Type.HybridAddressing,
                TimeCost = _options.TimeCost,
                MemoryCost = _options.MemoryCostMb * 1024,
                Lanes = _options.Parallelism,
                Threads = _options.Parallelism,
                Salt = salt,
                Password = Encoding.UTF8.GetBytes(plainTextSecret),
                HashLength = 32,
                Version = Argon2Version.Nineteen
            };

            using var argon2 = new Argon2(config);
            using var hash = argon2.Hash();

            return config.EncodeString(hash.Buffer);
        }

        public bool VerifySecret(string plainTextSecret, string hashedSecret)
        {
            if (string.IsNullOrWhiteSpace(plainTextSecret))
                throw new ArgumentException("Secret cannot be null or empty.", nameof(plainTextSecret));

            if (string.IsNullOrWhiteSpace(hashedSecret))
                throw new ArgumentException("Hashed secret cannot be null or empty.", nameof(hashedSecret));

            return Argon2.Verify(hashedSecret, plainTextSecret);
        }
    }
}