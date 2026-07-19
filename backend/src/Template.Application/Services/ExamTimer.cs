using HireExam.Core.Entities;

namespace HireExam.Application.Services;

internal static class ExamTimer
{
    public static int TotalSeconds(Exam exam) => exam.DurationMinutes * 60;

    public static int RemainingSeconds(Exam exam) =>
        Math.Max(0, TotalSeconds(exam) - exam.ElapsedSeconds);

    public static bool IsExpired(Exam exam) => exam.ElapsedSeconds >= TotalSeconds(exam);

    public static void ApplyElapsed(Exam exam, int? reportedElapsed)
    {
        if (!reportedElapsed.HasValue) return;
        var max = TotalSeconds(exam);
        exam.ElapsedSeconds = Math.Clamp(Math.Max(exam.ElapsedSeconds, reportedElapsed.Value), 0, max);
    }
}
