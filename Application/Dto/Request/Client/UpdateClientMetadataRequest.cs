namespace IbraHabra.NET.Application.Dto.Request.Client;

public class UpdateClientMetadataRequest
{
    public string? DisplayName { get; set; } 
    public string? ApplicationType { get; set; }
    public string? ConsentType { get; set; }
}