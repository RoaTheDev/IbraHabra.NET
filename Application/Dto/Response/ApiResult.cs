using System.Net;

namespace IbraHabra.NET.Application.Dto.Response;

public class ApiResult<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public string? Error { get; }

    public int StatusCode { get; }

    private ApiResult(T value)
    {
        IsSuccess = true;
        Value = value;
    }

    private ApiResult(int statusCode, T value)
    {
        IsSuccess = true;
        StatusCode = statusCode;
        Value = value;
    }

    private ApiResult(int status, string error)
    {
        IsSuccess = false;
        StatusCode = status;
        Error = error;
    }

    public static ApiResult<T> Ok(T value) => new(value);
    public static ApiResult<T> Ok(int statusCode, T value) => new(statusCode, value);
    public static ApiResult<T> Fail(int statusCode, string error) => new(statusCode, error);
}

public class ApiResult
{
    public bool IsSuccess { get; }
    public string? Error { get; }

    public int StatusCode { get; }

    private ApiResult(bool isSuccess)
    {
        IsSuccess = isSuccess;
    }

    private ApiResult(int statusCode, bool isSuccess)
    {
        StatusCode = statusCode;
        IsSuccess = isSuccess;
    }

    private ApiResult(bool isSuccess, int status, string error)
    {
        IsSuccess = isSuccess;
        StatusCode = status;
        Error = error;
    }

    public static ApiResult Ok() => new(true);
    public static ApiResult OK(int statusCode) => new(statusCode, true);
    public static ApiResult Fail(int statusCode, string error) => new(false, statusCode, error);
}