using HireExam.Core.Entities;

namespace HireExam.Application.Common;

public static class UserIdentity
{
    public static bool IsSuperAdmin(string? role) =>
        string.Equals(role, UserRoles.SuperAdmin, StringComparison.Ordinal);

    public static bool IsSameUser(string? left, string? right)
    {
        if (string.IsNullOrWhiteSpace(left) || string.IsNullOrWhiteSpace(right))
            return false;
        return string.Equals(left.Trim(), right.Trim(), StringComparison.OrdinalIgnoreCase);
    }

    public static bool CanAccessExam(string? userId, string? role, string? conductedBy) =>
        IsSuperAdmin(role) || IsSameUser(conductedBy, userId);
}
