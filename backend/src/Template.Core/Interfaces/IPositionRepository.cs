using HireExam.Core.Entities;

namespace HireExam.Core.Interfaces;

public interface IPositionRepository
{
    Task<IReadOnlyList<Position>> GetAllAsync(CancellationToken ct = default);
    Task<Position?> GetByIdAsync(string id, CancellationToken ct = default);
    Task InsertAsync(Position position, CancellationToken ct = default);
    Task<bool> DeleteAsync(string id, CancellationToken ct = default);
}

public interface IExamTemplateRepository
{
    Task<ExamTemplate?> GetByPositionIdAsync(string positionId, CancellationToken ct = default);
    Task InsertAsync(ExamTemplate template, CancellationToken ct = default);
    Task<bool> ReplaceAsync(ExamTemplate template, CancellationToken ct = default);
    Task<bool> DeleteByPositionIdAsync(string positionId, CancellationToken ct = default);
}

public interface IExamRepository
{
    Task<bool> AnyByPositionIdAsync(string positionId, CancellationToken ct = default);
}
