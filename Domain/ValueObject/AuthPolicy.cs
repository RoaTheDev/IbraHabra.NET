namespace IbraHabra.NET.Domain.ValueObject;

public class AuthPolicy
{
    public int MinPasswordLength { get; set; } = 8;
    public bool RequireDigit { get; set; } = false;
    public bool RequireUppercase { get; set; } = false;
    public bool RequireNonAlphanumeric { get; set; } = false;
    public bool RequireEmailVerification { get; set; } = false;
    public bool RequireMfa { get; set; } = false;
}