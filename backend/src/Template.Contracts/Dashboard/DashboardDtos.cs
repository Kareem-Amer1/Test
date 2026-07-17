namespace HireExam.Contracts.Dashboard;

public sealed record ExamsByPositionDto(string PositionId, string PositionName, int Count);

public sealed record DashboardStatsResponse(
    int TotalExams,
    int PendingGrading,
    int InProgress,
    int Graded,
    IReadOnlyList<ExamsByPositionDto> ExamsByPosition);
