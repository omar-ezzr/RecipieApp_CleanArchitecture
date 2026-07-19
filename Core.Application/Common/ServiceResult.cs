namespace Core.Application.Common;

public enum ServiceErrorType
{
    None,
    Validation,
    NotFound,
    Conflict
}

public sealed class ServiceResult<T>
{
    public bool IsSuccess { get; init; }
    public T? Value { get; init; }
    public string? Error { get; init; }
    public ServiceErrorType ErrorType { get; init; }

    public static ServiceResult<T> Success(T value) => new()
    {
        IsSuccess = true,
        Value = value
    };

    public static ServiceResult<T> Failure(string error, ServiceErrorType errorType) => new()
    {
        IsSuccess = false,
        Error = error,
        ErrorType = errorType
    };
}

public sealed class ServiceResult
{
    public bool IsSuccess { get; init; }
    public string? Error { get; init; }
    public ServiceErrorType ErrorType { get; init; }

    public static ServiceResult Success() => new()
    {
        IsSuccess = true
    };

    public static ServiceResult Failure(string error, ServiceErrorType errorType) => new()
    {
        IsSuccess = false,
        Error = error,
        ErrorType = errorType
    };
}
