using System.Security.Cryptography;

namespace IbraHabra.NET.Application.Utils;

public class AuthUtils
{
    public static bool TryCreateUri(string input, out Uri? uri)
    {
        if (Uri.TryCreate(input, UriKind.Absolute, out var created) &&
            (created.Scheme == Uri.UriSchemeHttp || created.Scheme == Uri.UriSchemeHttps))
        {
            uri = created;
            return true;
        }

        uri = null;
        return false;
    }

    public static string GenerateSecureSecret(int byteLength = 32)
    {
        var bytes = new byte[byteLength];
        RandomNumberGenerator.Fill(bytes);
        return Convert.ToBase64String(bytes);
    }
}