using System.Net;

namespace IbraHabra.NET.Application.Dto.Response;

public class ApiResult<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public string? Error { get; }

    public int StatusCode { get; }

    private ApiResult(bool isSuccess, T value)
    {
        IsSuccess = isSuccess;
        Value = value;
    }

    private ApiResult(bool isSuccess, int status, string error)
    {
        IsSuccess = isSuccess;
        StatusCode = status;
        Error = error;
    }

    public static ApiResult<T> Ok(T value) => new(true, value);
    public static ApiResult<T> Fail(int statusCode, string error) => new(false, statusCode, error);
}