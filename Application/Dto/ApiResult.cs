using IbraHabra.NET.Domain.Contract;

namespace IbraHabra.NET.Application.Dto;

public class ApiResult<T>
{
    public bool IsSuccess { get; }
    public T? Data { get; }
    public ApiError? ApiError { get; }

    private ApiResult(T data)
    {
        IsSuccess = true;
        Data = data;
    }

    private ApiResult(ApiError apiError)
    {
        IsSuccess = false;
        ApiError = apiError;
    }

    public static ApiResult<T> Success(T value) => new(value);
    public static ApiResult<T> Fail(ApiError apiError) => new(apiError);

    public TResult Match<TResult>(
        Func<T, TResult> onSuccess,
        Func<ApiError, TResult> onFailure)
    {
        return IsSuccess ? onSuccess(Data!) : onFailure(ApiError!);
    }
}

public class ApiResult
{
    public bool IsSuccess { get; }
    public ApiError? Error { get; }

    private ApiResult(bool isSuccess, ApiError? error = null)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    public static ApiResult Success() => new(true);
    public static ApiResult Fail(ApiError apiError) => new(false, apiError);
}

