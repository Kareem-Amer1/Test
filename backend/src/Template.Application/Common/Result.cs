namespace HireExam.Application.Common;

/// <summary>Typed error codes (Constitution VI). Stringly-typed strings are forbidden.</summary>
public enum ErrorCode
{
    None = 0,
    Validation,
    NotFound,
    Unauthorized,
    Forbidden,
    Conflict,
    Unexpected,
}

public readonly record struct Error(ErrorCode Code, string Message)
{
    public static readonly Error None = new(ErrorCode.None, string.Empty);
}

/// <summary>
/// Result of an application operation. Use <see cref="Success"/> /
/// <see cref="Failure(Error)"/>. Controllers map <see cref="Error.Code"/> to
/// HTTP status codes via <c>ResultToActionResult</c>.
/// </summary>
public readonly struct Result<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public Error Error { get; }

    private Result(bool ok, T? value, Error error)
    {
        IsSuccess = ok;
        Value = value;
        Error = error;
    }

    public static Result<T> Success(T value) => new(true, value, Common.Error.None);
    public static Result<T> Failure(Error error) => new(false, default, error);
    public static Result<T> Failure(ErrorCode code, string message) => new(false, default, new Error(code, message));
}
