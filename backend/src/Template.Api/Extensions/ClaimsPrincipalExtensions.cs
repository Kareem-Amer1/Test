using System.Security.Claims;
using HireExam.Core.Entities;

namespace HireExam.Api.Extensions;

public static class ClaimsPrincipalExtensions
{
    /// <summary>
    /// Prefer the JWT <c>sub</c> claim (what we issue). Fall back to NameIdentifier for legacy tokens.
    /// </summary>
    public static string GetUserId(this ClaimsPrincipal user) =>
        user.FindFirstValue("sub")
        ?? user.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? string.Empty;

    /// <summary>
    /// Read role from JWT <c>role</c> or mapped Role claim; default to HR when absent.
    /// </summary>
    public static string GetRole(this ClaimsPrincipal user)
    {
        var role = user.FindFirstValue("role")
            ?? user.FindFirstValue(ClaimTypes.Role);
        return string.IsNullOrWhiteSpace(role) ? UserRoles.HR : role;
    }
}
