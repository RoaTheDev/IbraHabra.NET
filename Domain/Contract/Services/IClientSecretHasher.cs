namespace IbraHabra.NET.Domain.Contract.Services;

public interface IClientSecretHasher
{
    string HashSecret(string plainTextSecret);
    bool VerifySecret(string plainTextSecret, string hashedSecret);
}