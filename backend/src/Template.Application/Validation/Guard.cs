using System.Text.RegularExpressions;

namespace HireExam.Application.Validation;

/// <summary>Constitution IV: all input validated at the API boundary OR here before reaching infra.</summary>
public static partial class Guard
{
    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$")]
    private static partial Regex EmailRegex();

    public static bool IsEmail(string? value) => !string.IsNullOrWhiteSpace(value) && EmailRegex().IsMatch(value);

    public static bool IsStrongEnoughPassword(string? value) =>
        !string.IsNullOrWhiteSpace(value) && value.Length >= 8;
}
