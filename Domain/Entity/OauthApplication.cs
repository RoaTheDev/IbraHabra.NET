using IbraHabra.NET.Domain.Interface;
using OpenIddict.EntityFrameworkCore.Models;

namespace IbraHabra.NET.Domain.Entity;

public class OauthApplication : OpenIddictEntityFrameworkCoreApplication, IEntity<string>
{
    public override string Id => base.Id!;

    public Guid ProjectId { get; set; }
    public virtual Projects Projects { get; set; } = null!;

    // 🌐 OAuth2 Settings
    public bool RequirePkce { get; set; } = true;
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}