using IbraHabra.NET.Domain.Contract;

namespace IbraHabra.NET.Application.Dto;

public class ApiResponse<T>
{
    public T? Data { get; set; }
    public List<ApiError>? Error { get; set; }
    public required ResponseMeta Meta { get; set; }
}
public class ApiResponse
{
    public object? Data { get; set; }
    public List<ApiError>? Error { get; set; }
    public required ResponseMeta Meta { get; set; }
}
public class ResponseMeta
{
    public required string RequestId { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string? Version { get; set; }
}
