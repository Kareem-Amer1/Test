namespace HireExam.Contracts.Common;

/// <summary>
/// Standard envelope returned by every endpoint (Constitution VII).
/// Shape matches the frontend's <c>ApiEnvelope&lt;T&gt;</c> in apiClient.ts.
/// </summary>
public sealed class ApiResponse<T>
{
    public bool Success { get; init; }
    public T? Data { get; init; }
    public string? Message { get; init; }
    public IReadOnlyList<string>? Errors { get; init; }

    public static ApiResponse<T> Ok(T data, string? message = null) =>
        new() { Success = true, Data = data, Message = message };

    public static ApiResponse<T> Fail(string code, string? message = null) =>
        new() { Success = false, Errors = new[] { code }, Message = message };

    public static ApiResponse<T> Fail(IReadOnlyList<string> errors, string? message = null) =>
        new() { Success = false, Errors = errors, Message = message };
}

public static class ApiResponse
{
    public static ApiResponse<object?> Ok(string? message = null) =>
        new() { Success = true, Message = message };
}
