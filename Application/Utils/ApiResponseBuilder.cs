using IbraHabra.NET.Application.Dto;

namespace IbraHabra.NET.Application.Utils;

public static class ApiResponseBuilder
{
    public static ApiResponse<T> Build<T>(HttpContext httpContext, ApiResult<T> result)
    {
        return new ApiResponse<T>
        {
            Data = result.Data,
            Error = result.ApiError != null ? [result.ApiError] : null,
            Meta = CreateMeta(httpContext)
        };
    }

    public static ApiResponse Build(HttpContext httpContext, ApiResult result)
    {
        return new ApiResponse
        {
            Data = null,
            Error = result.Error != null ? [result.Error] : null,
            Meta = CreateMeta(httpContext)
        };
    }

    public static ResponseMeta CreateMeta(HttpContext context)
        => new()
        {
            RequestId = context.TraceIdentifier,
            Version = context.GetRequestedApiVersion()?.ToString()
        };

    public static ResponseMeta CreateMeta(IHttpContextAccessor contextAccessor)
    {
        return new ResponseMeta
        {
            RequestId = contextAccessor.HttpContext?.TraceIdentifier ?? Guid.NewGuid().ToString(),
            Version = contextAccessor.HttpContext?.GetRequestedApiVersion()?.ToString()
        };
    }
}