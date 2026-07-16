using HireExam.Application.Common;
using HireExam.Contracts.Exams;
using HireExam.Core.Entities;
using HireExam.Core.Interfaces;

namespace HireExam.Application.Services;

public interface IExamService
{
    Task<Result<IReadOnlyList<ExamListItemDto>>> ListAsync(string userId, string role, CancellationToken ct = default);
    Task<Result<CreateExamResponse>> CreateAsync(string userId, CreateExamRequest request, CancellationToken ct = default);
    Task<Result<ExamSessionResponse>> GetSessionAsync(string examId, string userId, string role, CancellationToken ct = default);
    Task<Result<ExamSessionResponse>> SaveAnswersAsync(string examId, string userId, string role, SaveAnswersRequest request, CancellationToken ct = default);
    Task<Result<SubmitExamResponse>> SubmitAsync(string examId, string userId, string role, SubmitExamRequest? request, CancellationToken ct = default);
}

public sealed class ExamService : IExamService
{
    private readonly IExamRepository _exams;
    private readonly IPositionRepository _positions;
    private readonly IExamTemplateRepository _templates;
    private readonly IUserRepository _users;

    public ExamService(
        IExamRepository exams,
        IPositionRepository positions,
        IExamTemplateRepository templates,
        IUserRepository users)
    {
        _exams = exams;
        _positions = positions;
        _templates = templates;
        _users = users;
    }

    public async Task<Result<IReadOnlyList<ExamListItemDto>>> ListAsync(string userId, string role, CancellationToken ct = default)
    {
        var exams = role == UserRoles.SuperAdmin
            ? await _exams.ListAllAsync(ct)
            : await _exams.ListByConductedByAsync(userId, ct);

        var items = exams.Select(e => new ExamListItemDto(
            e.Id,
            e.CandidateName,
            e.PositionName,
            e.Status,
            e.StartedAt,
            e.SubmittedAt,
            e.TotalScore,
            e.MaxScore,
            e.ConductedByName)).ToList();

        return Result<IReadOnlyList<ExamListItemDto>>.Success(items);
    }

    public async Task<Result<CreateExamResponse>> CreateAsync(string userId, CreateExamRequest request, CancellationToken ct = default)
    {
        var candidateName = request.CandidateName?.Trim() ?? string.Empty;
        if (candidateName.Length < 2)
            return Result<CreateExamResponse>.Failure(ErrorCode.Validation, "exams.candidate_name_required");

        var position = await _positions.GetByIdAsync(request.PositionId, ct);
        if (position is null)
            return Result<CreateExamResponse>.Failure(ErrorCode.NotFound, "positions.not_found");

        var template = await _templates.GetByPositionIdAsync(request.PositionId, ct);
        if (template is null || template.Questions.Count == 0)
            return Result<CreateExamResponse>.Failure(ErrorCode.Validation, "exams.template_empty");

        var user = await _users.GetByIdAsync(userId, ct);
        var snapshot = template.Questions
            .OrderBy(q => q.Order)
            .Select(q => new ExamQuestionSnapshot
            {
                Id = q.Id,
                Type = q.Type,
                Text = q.Text,
                Points = q.Points,
                Choices = q.Choices?.Select(c => new McqChoice { Id = c.Id, Text = c.Text }).ToList(),
                CorrectAnswer = q.CorrectAnswer,
                CorrectChoiceId = q.CorrectChoiceId,
            })
            .ToList();

        var exam = new Exam
        {
            CandidateName = candidateName,
            PositionId = position.Id,
            PositionName = position.Name,
            ConductedBy = userId,
            ConductedByName = user?.FullName ?? user?.Email ?? "HR",
            DurationMinutes = template.DurationMinutes,
            StartedAt = DateTime.UtcNow,
            Status = ExamStatuses.InProgress,
            QuestionsSnapshot = snapshot,
            Answers = snapshot.Select(q => new ExamAnswer { QuestionId = q.Id }).ToList(),
            MaxScore = snapshot.Sum(q => q.Points),
            AutoGradedScore = 0,
            IsFullyGraded = false,
        };

        await _exams.InsertAsync(exam, ct);
        return Result<CreateExamResponse>.Success(new CreateExamResponse(
            exam.Id, exam.CandidateName, exam.PositionName, exam.Status));
    }

    public async Task<Result<ExamSessionResponse>> GetSessionAsync(string examId, string userId, string role, CancellationToken ct = default)
    {
        var exam = await LoadAuthorizedAsync(examId, userId, role, ct);
        if (exam is null)
            return Result<ExamSessionResponse>.Failure(ErrorCode.NotFound, "exams.not_found");

        return Result<ExamSessionResponse>.Success(MapSession(exam));
    }

    public async Task<Result<ExamSessionResponse>> SaveAnswersAsync(
        string examId, string userId, string role, SaveAnswersRequest request, CancellationToken ct = default)
    {
        var exam = await LoadAuthorizedAsync(examId, userId, role, ct);
        if (exam is null)
            return Result<ExamSessionResponse>.Failure(ErrorCode.NotFound, "exams.not_found");
        if (exam.Status != ExamStatuses.InProgress)
            return Result<ExamSessionResponse>.Failure(ErrorCode.Conflict, "exams.not_in_progress");
        if (IsTimeExpired(exam))
            return Result<ExamSessionResponse>.Failure(ErrorCode.Conflict, "exams.time_expired");

        if (request.Answers is null || request.Answers.Count == 0)
            return Result<ExamSessionResponse>.Failure(ErrorCode.Validation, "exams.answers_required");

        MergeAnswers(exam, request.Answers);
        if (!await _exams.ReplaceAsync(exam, ct))
            return Result<ExamSessionResponse>.Failure(ErrorCode.NotFound, "exams.not_found");

        return Result<ExamSessionResponse>.Success(MapSession(exam));
    }

    public async Task<Result<SubmitExamResponse>> SubmitAsync(
        string examId, string userId, string role, SubmitExamRequest? request, CancellationToken ct = default)
    {
        var exam = await LoadAuthorizedAsync(examId, userId, role, ct);
        if (exam is null)
            return Result<SubmitExamResponse>.Failure(ErrorCode.NotFound, "exams.not_found");
        if (exam.Status != ExamStatuses.InProgress)
            return Result<SubmitExamResponse>.Failure(ErrorCode.Conflict, "exams.already_submitted");

        if (request?.Answers is { Count: > 0 })
            MergeAnswers(exam, request.Answers);

        exam.Status = ExamStatuses.Submitted;
        exam.SubmittedAt = DateTime.UtcNow;
        if (!await _exams.ReplaceAsync(exam, ct))
            return Result<SubmitExamResponse>.Failure(ErrorCode.NotFound, "exams.not_found");

        return Result<SubmitExamResponse>.Success(new SubmitExamResponse(
            exam.Id, exam.Status, exam.SubmittedAt.Value));
    }

    private async Task<Exam?> LoadAuthorizedAsync(string examId, string userId, string role, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(examId)) return null;
        var exam = await _exams.GetByIdAsync(examId, ct);
        if (exam is null) return null;
        if (!CanAccess(exam, userId, role)) return null;
        return exam;
    }

    private static bool CanAccess(Exam exam, string userId, string role) =>
        role == UserRoles.SuperAdmin || exam.ConductedBy == userId;

    private static bool IsTimeExpired(Exam exam)
    {
        var end = exam.StartedAt.AddMinutes(exam.DurationMinutes);
        return DateTime.UtcNow > end;
    }

    private static void MergeAnswers(Exam exam, IReadOnlyList<ExamAnswerInputDto> incoming)
    {
        var lookup = exam.Answers.ToDictionary(a => a.QuestionId);
        foreach (var input in incoming)
        {
            if (!lookup.TryGetValue(input.QuestionId, out var answer))
                continue;
            answer.EssayText = input.EssayText;
            answer.TrueFalseAnswer = input.TrueFalseAnswer;
            answer.SelectedChoiceId = input.SelectedChoiceId;
        }
    }

    internal static ExamSessionResponse MapSession(Exam exam)
    {
        var questions = exam.QuestionsSnapshot
            .Select((q, i) => new ExamSessionQuestionDto(
                q.Id,
                q.Type,
                q.Text,
                q.Points,
                q.Choices?.Select(c => new ExamSessionChoiceDto(c.Id, c.Text)).ToList(),
                i))
            .ToList();

        var answers = exam.Answers.Select(a => new ExamAnswerInputDto(
            a.QuestionId,
            a.EssayText,
            a.TrueFalseAnswer,
            a.SelectedChoiceId)).ToList();

        return new ExamSessionResponse(
            exam.Id,
            exam.CandidateName,
            exam.PositionName,
            exam.DurationMinutes,
            exam.StartedAt,
            exam.Status,
            questions,
            answers);
    }
}
