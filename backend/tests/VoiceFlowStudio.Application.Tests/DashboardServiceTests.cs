using Moq;
using HireExam.Application.Services;
using HireExam.Core.Entities;
using HireExam.Core.Interfaces;
using Xunit;

namespace HireExam.Application.Tests;

public class DashboardServiceTests
{
    [Fact]
    public async Task GetStats_Hr_ScopesCountsToUser()
    {
        const string hrId = "674abc123456789012345678";
        var exams = new Mock<IExamRepository>();
        ExamListFilter? scopeFilter = null;

        exams.Setup(e => e.CountAsync(It.IsAny<ExamListFilter>(), default))
            .Callback<ExamListFilter, CancellationToken>((f, _) =>
            {
                if (f.PendingGradingOnly || f.Status is not null) return;
                scopeFilter ??= f;
            })
            .ReturnsAsync(0);

        exams.Setup(e => e.CountByPositionAsync(It.IsAny<ExamListFilter>(), default))
            .ReturnsAsync(Array.Empty<PositionExamCount>());

        var svc = new DashboardService(exams.Object);
        var result = await svc.GetStatsAsync(hrId, UserRoles.HR);

        Assert.True(result.IsSuccess);
        Assert.NotNull(scopeFilter);
        Assert.Equal(hrId, scopeFilter!.ConductedBy);
    }

    [Fact]
    public async Task GetStats_SuperAdmin_DoesNotScopeConductedBy()
    {
        var exams = new Mock<IExamRepository>();
        ExamListFilter? scopeFilter = null;

        exams.Setup(e => e.CountAsync(It.IsAny<ExamListFilter>(), default))
            .Callback<ExamListFilter, CancellationToken>((f, _) =>
            {
                if (f.PendingGradingOnly || f.Status is not null) return;
                scopeFilter ??= f;
            })
            .ReturnsAsync(5);

        exams.Setup(e => e.CountByPositionAsync(It.IsAny<ExamListFilter>(), default))
            .ReturnsAsync(Array.Empty<PositionExamCount>());

        var svc = new DashboardService(exams.Object);
        var result = await svc.GetStatsAsync("admin-id", UserRoles.SuperAdmin);

        Assert.True(result.IsSuccess);
        Assert.Equal(5, result.Value!.TotalExams);
        Assert.NotNull(scopeFilter);
        Assert.Null(scopeFilter!.ConductedBy);
    }
}
