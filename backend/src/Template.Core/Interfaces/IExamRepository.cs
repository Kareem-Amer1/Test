using HireExam.Core.Entities;

namespace HireExam.Core.Interfaces;

public sealed class ExamListFilter
{
    public string? ConductedBy { get; init; }
    public string? PositionId { get; init; }
    public string? Status { get; init; }
    public DateTime? From { get; init; }
    public DateTime? To { get; init; }
}

public interface IExamRepository
{
    Task<Exam?> GetByIdAsync(string id, CancellationToken ct = default);
    Task<IReadOnlyList<Exam>> ListAsync(ExamListFilter filter, CancellationToken ct = default);
    Task InsertAsync(Exam exam, CancellationToken ct = default);
    Task<bool> ReplaceAsync(Exam exam, CancellationToken ct = default);
    Task<bool> AnyByPositionIdAsync(string positionId, CancellationToken ct = default);
}
