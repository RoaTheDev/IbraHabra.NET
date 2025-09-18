using IbraHabra.NET.Domain.enums;

namespace IbraHabra.NET.Domain.Entity;

public class Client
{
    public Guid Id { get; set; } // Internal ID
    public string ClientId { get; set; } = Guid.NewGuid().ToString();
    public ClientTypeEnum ClientType { get; set; } = ClientTypeEnum.Web;

    public Guid ProjectId { get; set; } // 👈 Match Project.ProjectId type
    public Project Project { get; set; } = null!;

    // 🔐 Auth Policies (per client)
    public int MinPasswordLength { get; set; } = 6;
    public bool RequireDigit { get; set; } = false;
    public bool RequireUppercase { get; set; } = false;
    public bool RequireNonAlphanumeric { get; set; } = false;
    public bool RequireEmailVerification { get; set; } = false;
    public bool RequireMfa { get; set; } = false;

    // 🌐 OAuth2 Settings
    public List<string> RedirectUris { get; set; } = new();
    public List<string> AllowedScopes { get; set; } = new() { "openid", "profile", "email" };
    public bool RequirePkce { get; set; } = true; // Default for SPA/Mobile
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}