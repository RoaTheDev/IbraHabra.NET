using OpenIddict.EntityFrameworkCore.Models;

namespace IbraHabra.NET.Domain.Entity;

public class OauthApplication : OpenIddictEntityFrameworkCoreApplication<Guid, Role, OpenIddictEntityFrameworkCoreToken<Guid>>
{
    public Guid ProjectId { get; set; }
    public virtual Projects Projects { get; set; } = null!;

    // 🔐 Auth Policies (per client)
    public int MinPasswordLength { get; set; } = 8;
    public bool RequireDigit { get; set; }
    public bool RequireUppercase { get; set; }
    public bool RequireNonAlphanumeric { get; set; }
    public bool RequireEmailVerification { get; set; }
    public bool RequireMfa { get; set; }

    // 🌐 OAuth2 Settings

    public bool RequirePkce { get; set; } = true;
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}