using HireExam.Core.Entities;

namespace HireExam.Core.Interfaces;

public interface IExamRepository
{
    Task<Exam?> GetByIdAsync(string id, CancellationToken ct = default);
    Task<IReadOnlyList<Exam>> ListByConductedByAsync(string userId, CancellationToken ct = default);
    Task<IReadOnlyList<Exam>> ListAllAsync(CancellationToken ct = default);
    Task InsertAsync(Exam exam, CancellationToken ct = default);
    Task<bool> ReplaceAsync(Exam exam, CancellationToken ct = default);
    Task<bool> AnyByPositionIdAsync(string positionId, CancellationToken ct = default);
}
