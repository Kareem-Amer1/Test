using HireExam.Application.Common;
using HireExam.Contracts.Dashboard;
using HireExam.Core.Entities;
using HireExam.Core.Interfaces;

namespace HireExam.Application.Services;

public interface IDashboardService
{
    Task<Result<DashboardStatsResponse>> GetStatsAsync(string userId, string role, CancellationToken ct = default);
}

public sealed class DashboardService : IDashboardService
{
    private readonly IExamRepository _exams;

    public DashboardService(IExamRepository exams) => _exams = exams;

    public async Task<Result<DashboardStatsResponse>> GetStatsAsync(string userId, string role, CancellationToken ct = default)
    {
        var scope = new ExamListFilter
        {
            ConductedBy = UserIdentity.IsSuperAdmin(role) ? null : userId,
        };

        var total = await _exams.CountAsync(scope, ct);
        var pending = await _exams.CountAsync(new ExamListFilter
        {
            ConductedBy = scope.ConductedBy,
            PendingGradingOnly = true,
        }, ct);
        var inProgress = await _exams.CountAsync(new ExamListFilter
        {
            ConductedBy = scope.ConductedBy,
            Status = ExamStatuses.InProgress,
        }, ct);
        var graded = await _exams.CountAsync(new ExamListFilter
        {
            ConductedBy = scope.ConductedBy,
            Status = ExamStatuses.Graded,
        }, ct);
        var byPosition = await _exams.CountByPositionAsync(scope, ct);

        var response = new DashboardStatsResponse(
            (int)total,
            (int)pending,
            (int)inProgress,
            (int)graded,
            byPosition.Select(p => new ExamsByPositionDto(p.PositionId, p.PositionName, (int)p.Count)).ToList());

        return Result<DashboardStatsResponse>.Success(response);
    }
}
