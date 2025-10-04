using IbraHabra.NET.Domain.Contract;
using Microsoft.AspNetCore.Identity;

namespace IbraHabra.NET.Domain.Entities;

public class Role : IdentityRole<Guid>, IEntity<Guid>
{
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; }
}