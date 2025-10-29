namespace IbraHabra.NET.Domain.Contract;

public class CorsSettings
{
    public string[] AdminOrigins { get; set; } = [];
    public bool EnableDevelopmentCors { get; set; } = false;
    public string[] DevelopmentOrigins { get; set; } = [];
}