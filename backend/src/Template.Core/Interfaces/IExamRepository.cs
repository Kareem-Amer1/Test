using HireExam.Core.Entities;

namespace HireExam.Core.Interfaces;

public sealed class ExamListFilter
{
    public string? ConductedBy { get; init; }
    public string? PositionId { get; init; }
    public string? Status { get; init; }
    public DateTime? From { get; init; }
    public DateTime? To { get; init; }
    public string? Search { get; init; }
    public bool PendingGradingOnly { get; init; }
}

public sealed class PositionExamCount
{
    public string PositionId { get; init; } = string.Empty;
    public string PositionName { get; init; } = string.Empty;
    public long Count { get; init; }
}

public interface IExamRepository
{
    Task<Exam?> GetByIdAsync(string id, CancellationToken ct = default);
    Task<IReadOnlyList<Exam>> ListAsync(ExamListFilter filter, CancellationToken ct = default);
    Task<long> CountAsync(ExamListFilter filter, CancellationToken ct = default);
    Task<IReadOnlyList<PositionExamCount>> CountByPositionAsync(ExamListFilter filter, CancellationToken ct = default);
    Task InsertAsync(Exam exam, CancellationToken ct = default);
    Task<bool> ReplaceAsync(Exam exam, CancellationToken ct = default);
    Task<bool> AnyByPositionIdAsync(string positionId, CancellationToken ct = default);
}
