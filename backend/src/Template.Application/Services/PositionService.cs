using HireExam.Application.Common;
using HireExam.Contracts.Positions;
using HireExam.Core.Entities;
using HireExam.Core.Interfaces;

namespace HireExam.Application.Services;

public interface IPositionService
{
    Task<Result<IReadOnlyList<PositionResponse>>> ListAsync(CancellationToken ct = default);
    Task<Result<PositionResponse>> CreateAsync(string userId, CreatePositionRequest request, CancellationToken ct = default);
    Task<Result<bool>> DeleteAsync(string id, CancellationToken ct = default);
}

public sealed class PositionService : IPositionService
{
    private readonly IPositionRepository _positions;
    private readonly IExamTemplateRepository _templates;
    private readonly IExamRepository _exams;

    public PositionService(
        IPositionRepository positions,
        IExamTemplateRepository templates,
        IExamRepository exams)
    {
        _positions = positions;
        _templates = templates;
        _exams = exams;
    }

    public async Task<Result<IReadOnlyList<PositionResponse>>> ListAsync(CancellationToken ct = default)
    {
        var items = await _positions.GetAllAsync(ct);
        var list = items.Select(Map).ToList();
        return Result<IReadOnlyList<PositionResponse>>.Success(list);
    }

    public async Task<Result<PositionResponse>> CreateAsync(string userId, CreatePositionRequest request, CancellationToken ct = default)
    {
        var name = request.Name?.Trim() ?? string.Empty;
        if (name.Length < 2)
            return Result<PositionResponse>.Failure(ErrorCode.Validation, "positions.name_required");

        var position = new Position
        {
            Name = name,
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
            IsActive = true,
            CreatedBy = userId,
        };
        await _positions.InsertAsync(position, ct);

        await _templates.InsertAsync(new ExamTemplate
        {
            PositionId = position.Id,
            DurationMinutes = 60,
            Partitions = new List<TemplatePartition>(),
            LastModifiedAt = DateTime.UtcNow,
            LastModifiedBy = userId,
        }, ct);

        return Result<PositionResponse>.Success(Map(position));
    }

    public async Task<Result<bool>> DeleteAsync(string id, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(id))
            return Result<bool>.Failure(ErrorCode.Validation, "positions.id_required");

        var position = await _positions.GetByIdAsync(id, ct);
        if (position is null)
            return Result<bool>.Failure(ErrorCode.NotFound, "positions.not_found");

        if (await _exams.AnyByPositionIdAsync(id, ct))
            return Result<bool>.Failure(ErrorCode.Conflict, "positions.has_exams");

        await _templates.DeleteByPositionIdAsync(id, ct);
        var deleted = await _positions.DeleteAsync(id, ct);
        if (!deleted)
            return Result<bool>.Failure(ErrorCode.NotFound, "positions.not_found");

        return Result<bool>.Success(true);
    }

    private static PositionResponse Map(Position p) =>
        new(p.Id, p.Name, p.Description, p.IsActive, p.CreatedAt);
}
