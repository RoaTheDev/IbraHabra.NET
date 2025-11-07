namespace IbraHabra.NET.Domain.Constants.Values;

public class AuthPolicy
{
    public int MinPasswordLength { get; set; } = 8;
    public bool RequireDigit { get; set; }
    public bool RequireUppercase { get; set; }
    public bool RequireNonAlphanumeric { get; set; }
    public bool RequireEmailVerification { get; set; }
    public bool RequireMfa { get; set; }
    
}