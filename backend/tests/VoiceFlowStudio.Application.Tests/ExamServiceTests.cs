using Moq;
using HireExam.Application.Common;
using HireExam.Application.Services;
using HireExam.Contracts.Exams;
using HireExam.Core.Entities;
using HireExam.Core.Interfaces;
using Xunit;

namespace HireExam.Application.Tests;

public class ExamServiceTests
{
    private const string Hr1 = "674abc123456789012345678";
    private const string Hr2 = "674def123456789012345678";

    private static (ExamService svc, Mock<IExamRepository> exams) Build()
    {
        var exams = new Mock<IExamRepository>();
        var svc = new ExamService(exams.Object);
        return (svc, exams);
    }

    private static Exam SampleExam(string conductedBy, string status = ExamStatuses.Submitted) => new()
    {
        Id = "674111123456789012345678",
        CandidateName = "Candidate",
        PositionId = "674222123456789012345678",
        PositionName = "Engineer",
        ConductedBy = conductedBy,
        ConductedByName = "HR User",
        DurationMinutes = 60,
        StartedAt = DateTime.UtcNow.AddMinutes(-30),
        SubmittedAt = status == ExamStatuses.InProgress ? null : DateTime.UtcNow,
        Status = status,
        QuestionsSnapshot = [new ExamQuestionSnapshot { Id = "q1", Type = QuestionTypes.Mcq, Text = "Q", Points = 10, CorrectChoiceId = "c1" }],
        Answers = [new ExamAnswer { QuestionId = "q1", SelectedChoiceId = "c1" }],
        MaxScore = 10,
    };

    [Fact]
    public async Task GetSession_HrOwnExam_ReturnsSession()
    {
        var (svc, exams) = Build();
        var exam = SampleExam(Hr1, ExamStatuses.InProgress);
        exams.Setup(e => e.GetByIdAsync(exam.Id, default)).ReturnsAsync(exam);

        var result = await svc.GetSessionAsync(exam.Id, Hr1, UserRoles.HR);

        Assert.True(result.IsSuccess);
        Assert.Equal(exam.Id, result.Value!.Id);
    }

    [Fact]
    public async Task GetSession_HrOtherExam_ReturnsForbidden()
    {
        var (svc, exams) = Build();
        var exam = SampleExam(Hr2, ExamStatuses.InProgress);
        exams.Setup(e => e.GetByIdAsync(exam.Id, default)).ReturnsAsync(exam);

        var result = await svc.GetSessionAsync(exam.Id, Hr1, UserRoles.HR);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCode.Forbidden, result.Error.Code);
        Assert.Equal("exams.forbidden", result.Error.Message);
    }

    [Fact]
    public async Task GetSession_SuperAdmin_ReturnsOtherHrExam()
    {
        var (svc, exams) = Build();
        var exam = SampleExam(Hr2, ExamStatuses.InProgress);
        exams.Setup(e => e.GetByIdAsync(exam.Id, default)).ReturnsAsync(exam);

        var result = await svc.GetSessionAsync(exam.Id, Hr1, UserRoles.SuperAdmin);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task GetDetail_InProgress_ReturnsConflict()
    {
        var (svc, exams) = Build();
        var exam = SampleExam(Hr1, ExamStatuses.InProgress);
        exams.Setup(e => e.GetByIdAsync(exam.Id, default)).ReturnsAsync(exam);

        var result = await svc.GetDetailAsync(exam.Id, Hr1, UserRoles.HR);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCode.Conflict, result.Error.Code);
        Assert.Equal("exams.still_in_progress", result.Error.Message);
    }

    [Fact]
    public async Task GetDetail_HrOtherExam_ReturnsForbidden()
    {
        var (svc, exams) = Build();
        var exam = SampleExam(Hr2);
        exams.Setup(e => e.GetByIdAsync(exam.Id, default)).ReturnsAsync(exam);

        var result = await svc.GetDetailAsync(exam.Id, Hr1, UserRoles.HR);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCode.Forbidden, result.Error.Code);
    }

    [Fact]
    public async Task ListAsync_Hr_ScopesToConductedBy()
    {
        var (svc, exams) = Build();
        ExamListFilter? captured = null;
        exams.Setup(e => e.ListAsync(It.IsAny<ExamListFilter>(), default))
            .Callback<ExamListFilter, CancellationToken>((f, _) => captured = f)
            .ReturnsAsync(Array.Empty<Exam>());

        await svc.ListAsync(Hr1, UserRoles.HR, null);

        Assert.NotNull(captured);
        Assert.Equal(Hr1, captured!.ConductedBy);
    }

    [Fact]
    public async Task ListAsync_SuperAdmin_DoesNotScopeConductedBy()
    {
        var (svc, exams) = Build();
        ExamListFilter? captured = null;
        exams.Setup(e => e.ListAsync(It.IsAny<ExamListFilter>(), default))
            .Callback<ExamListFilter, CancellationToken>((f, _) => captured = f)
            .ReturnsAsync(Array.Empty<Exam>());

        await svc.ListAsync(Hr1, UserRoles.SuperAdmin, null);

        Assert.NotNull(captured);
        Assert.Null(captured!.ConductedBy);
    }

    [Fact]
    public async Task SubmitAsync_AlreadySubmitted_ReturnsConflict()
    {
        var (svc, exams) = Build();
        var exam = SampleExam(Hr1);
        exams.Setup(e => e.GetByIdAsync(exam.Id, default)).ReturnsAsync(exam);

        var result = await svc.SubmitAsync(exam.Id, Hr1, UserRoles.HR, null);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCode.Conflict, result.Error.Code);
        Assert.Equal("exams.already_submitted", result.Error.Message);
    }

    [Fact]
    public async Task SaveAnswers_ExpiredTimer_ReturnsConflict()
    {
        var (svc, exams) = Build();
        var exam = SampleExam(Hr1, ExamStatuses.InProgress);
        exam.DurationMinutes = 30;
        exam.ElapsedSeconds = 30 * 60;
        exams.Setup(e => e.GetByIdAsync(exam.Id, default)).ReturnsAsync(exam);

        var result = await svc.SaveAnswersAsync(
            exam.Id,
            Hr1,
            UserRoles.HR,
            new SaveAnswersRequest([new ExamAnswerInputDto("q1", null, null, "c1")]));

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCode.Conflict, result.Error.Code);
        Assert.Equal("exams.time_expired", result.Error.Message);
    }

    [Fact]
    public async Task GradeAsync_FullyGraded_ReturnsConflict()
    {
        var (svc, exams) = Build();
        var exam = SampleExam(Hr1);
        exam.Status = ExamStatuses.Graded;
        exam.IsFullyGraded = true;
        exam.TotalScore = 10;
        exams.Setup(e => e.GetByIdAsync(exam.Id, default)).ReturnsAsync(exam);

        var result = await svc.GradeAsync(
            exam.Id,
            Hr1,
            UserRoles.HR,
            new GradeExamRequest([new EssayScoreInputDto("e1", 5)], Finalize: true));

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCode.Conflict, result.Error.Code);
        Assert.Equal("exams.already_graded", result.Error.Message);
    }
}
