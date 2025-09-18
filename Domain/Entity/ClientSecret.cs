namespace IbraHabra.NET.Domain.Entity;

public class ClientSecret 
{
    public int Id { get; set; }
    public int ClientId { get; set; } 
    public string HashedSecret { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ExpiresAt { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Description { get; set; }

    public Client Client { get; set; } = default!;
}