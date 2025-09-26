using IbraHabra.NET.Domain.Interface;
using Microsoft.AspNetCore.Identity;

namespace IbraHabra.NET.Domain.Entity;

public class Role : IdentityRole<Guid>, IEntity<Guid>
{
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; }
}