namespace IbraHabra.NET.Infra.Middleware;

public class DynamicCorsMiddleware
{
    private readonly RequestDelegate _next;

    public DynamicCorsMiddleware(RequestDelegate @delegate)
    {
        _next = @delegate;
    }
}