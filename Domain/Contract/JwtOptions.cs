namespace IbraHabra.NET.Domain.Contract;

public class JwtOptions
{
    public string Secret { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public bool RequireHttps { get; set; } = true;
}