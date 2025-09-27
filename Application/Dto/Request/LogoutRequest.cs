namespace IbraHabra.NET.Application.Dto.Request;

public class LogoutRequest
{
    public string ClientId { get; set; } = null!;
    public bool RevokeAllToken { get; set; } = false;
}