using Microsoft.AspNetCore.Mvc;
using HireExam.Api.Localization;
using HireExam.Application.Common;
using HireExam.Contracts.Common;

namespace HireExam.Api.Middleware;

/// <summary>
/// Shared utility (Constitution VI) that maps <see cref="Result{T}"/> to
/// HTTP responses using the <see cref="ApiResponse{T}"/> envelope.
/// </summary>
public static class ResultMapper
{
    public static IActionResult ToActionResult<T>(this Result<T> result, ILocalizer loc, int successStatus = StatusCodes.Status200OK)
    {
        if (result.IsSuccess)
        {
            return new ObjectResult(ApiResponse<T>.Ok(result.Value!))
            {
                StatusCode = successStatus,
            };
        }

        var status = result.Error.Code switch
        {
            ErrorCode.Validation => StatusCodes.Status400BadRequest,
            ErrorCode.Unauthorized => StatusCodes.Status401Unauthorized,
            ErrorCode.Forbidden => StatusCodes.Status403Forbidden,
            ErrorCode.NotFound => StatusCodes.Status404NotFound,
            ErrorCode.Conflict => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status500InternalServerError,
        };
        return new ObjectResult(ApiResponse<T>.Fail(result.Error.Message, loc.Translate(result.Error.Message)))
        {
            StatusCode = status,
        };
    }
}
