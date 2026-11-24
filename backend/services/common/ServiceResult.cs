namespace backend.services.common;

public enum ServiceResultStatus
{
    Success,
    Invalid,
    NotFound,
    Unauthorized
}

public sealed class ServiceResult<T> where T : class
{
    public ServiceResultStatus Status { get; }
    public T? Value { get; }
    public string? Error { get; }

    private ServiceResult(
        ServiceResultStatus status,
        T? value,
        string? error)
    {
        Status = status;
        Value = value;
        Error = error;
    }

    public static ServiceResult<T> Success(T value)
    {
        ArgumentNullException.ThrowIfNull(value);

        return new ServiceResult<T>(
            ServiceResultStatus.Success,
            value,
            null);
    }

    public static ServiceResult<T> Invalid(string error)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(error);

        return new ServiceResult<T>(
            ServiceResultStatus.Invalid,
            null,
            error);
    }

    public static ServiceResult<T> NotFound()
    {
        return new ServiceResult<T>(
            ServiceResultStatus.NotFound,
            null,
            null);
    }

    public static ServiceResult<T> Unauthorized(string error)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(error);

        return new ServiceResult<T>(
            ServiceResultStatus.Unauthorized,
            null,
            error);
    }
}