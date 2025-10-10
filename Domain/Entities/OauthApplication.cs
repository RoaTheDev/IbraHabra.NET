using IbraHabra.NET.Domain.Contract;
using OpenIddict.EntityFrameworkCore.Models;

namespace IbraHabra.NET.Domain.Entities;

public class OauthApplication : OpenIddictEntityFrameworkCoreApplication, IEntity<string>
{
    public override string Id { get; set; } = null!;
    public Guid ProjectId { get; set; }
    public virtual Projects Projects { get; set; } = null!;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}