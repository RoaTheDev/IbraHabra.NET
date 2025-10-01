namespace IbraHabra.NET.Application.Utils;

public class TwoFactorUtils
{
    public static string GenerateQrCodeUri(string email, string key,string name)
    {
        const string format = "otpauth://totp/{0}:{1}?secret={2}&issuer={0}&digits=6";
        var encoder = System.Text.Encodings.Web.UrlEncoder.Default;
        
        return string.Format(format,
            encoder.Encode(name),
            encoder.Encode(email),
            key);
    }

    public static string FormatKey(string key)
    {
        var result = new System.Text.StringBuilder();
        for (int i = 0; i < key.Length; i += 4)
        {
            if (i > 0) result.Append(' ');
            
            int charsToTake = Math.Min(4, key.Length - i);
            result.Append(key.Substring(i, charsToTake));
        }
        return result.ToString().ToLowerInvariant();
    }
}