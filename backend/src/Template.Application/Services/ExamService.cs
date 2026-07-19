using HireExam.Application.Common;
using HireExam.Contracts.Exams;
using HireExam.Core.Entities;
using HireExam.Core.Interfaces;

namespace HireExam.Application.Services;

public interface IExamService
{
    Task<Result<IReadOnlyList<ExamListItemDto>>> ListAsync(
        string userId, string role, ExamListQueryDto? query, CancellationToken ct = default);
    Task<Result<ExamSessionResponse>> GetSessionAsync(string examId, string userId, string role, CancellationToken ct = default);
    Task<Result<ExamDetailResponse>> GetDetailAsync(string examId, string userId, string role, CancellationToken ct = default);
    Task<Result<ExamSessionResponse>> SaveAnswersAsync(string examId, string userId, string role, SaveAnswersRequest request, CancellationToken ct = default);
    Task<Result<SubmitExamResponse>> SubmitAsync(string examId, string userId, string role, SubmitExamRequest? request, CancellationToken ct = default);
    Task<Result<GradeExamResponse>> GradeAsync(string examId, string userId, string role, GradeExamRequest request, CancellationToken ct = default);
}

public sealed class ExamService : IExamService
{
    private readonly IExamRepository _exams;

    public ExamService(IExamRepository exams)
    {
        _exams = exams;
    }

    public async Task<Result<IReadOnlyList<ExamListItemDto>>> ListAsync(
        string userId, string role, ExamListQueryDto? query, CancellationToken ct = default)
    {
        var filter = new ExamListFilter
        {
            ConductedBy = UserIdentity.IsSuperAdmin(role) ? null : userId,
            PositionId = query?.PositionId,
            Status = query?.Status,
            From = query?.From,
            To = query?.To,
            Search = query?.Search,
        };

        var exams = await _exams.ListAsync(filter, ct);
        var items = exams.Select(MapListItem).ToList();
        return Result<IReadOnlyList<ExamListItemDto>>.Success(items);
    }

    public async Task<Result<ExamSessionResponse>> GetSessionAsync(string examId, string userId, string role, CancellationToken ct = default)
    {
        var access = await LoadAuthorizedAsync(examId, userId, role, ct);
        if (access.AccessDenied)
            return Result<ExamSessionResponse>.Failure(ErrorCode.Forbidden, "exams.forbidden");
        if (access.Exam is null)
            return Result<ExamSessionResponse>.Failure(ErrorCode.NotFound, "exams.not_found");

        return Result<ExamSessionResponse>.Success(MapSession(access.Exam));
    }

    public async Task<Result<ExamDetailResponse>> GetDetailAsync(string examId, string userId, string role, CancellationToken ct = default)
    {
        var access = await LoadAuthorizedAsync(examId, userId, role, ct);
        if (access.AccessDenied)
            return Result<ExamDetailResponse>.Failure(ErrorCode.Forbidden, "exams.forbidden");
        if (access.Exam is null)
            return Result<ExamDetailResponse>.Failure(ErrorCode.NotFound, "exams.not_found");
        if (access.Exam.Status == ExamStatuses.InProgress)
            return Result<ExamDetailResponse>.Failure(ErrorCode.Conflict, "exams.still_in_progress");

        return Result<ExamDetailResponse>.Success(MapDetail(access.Exam));
    }

    public async Task<Result<ExamSessionResponse>> SaveAnswersAsync(
        string examId, string userId, string role, SaveAnswersRequest request, CancellationToken ct = default)
    {
        var access = await LoadAuthorizedAsync(examId, userId, role, ct);
        if (access.AccessDenied)
            return Result<ExamSessionResponse>.Failure(ErrorCode.Forbidden, "exams.forbidden");
        if (access.Exam is null)
            return Result<ExamSessionResponse>.Failure(ErrorCode.NotFound, "exams.not_found");

        var exam = access.Exam;
        if (exam.Status != ExamStatuses.InProgress)
            return Result<ExamSessionResponse>.Failure(ErrorCode.Conflict, "exams.not_in_progress");
        ExamTimer.ApplyElapsed(exam, request.ElapsedSeconds);
        if (ExamTimer.IsExpired(exam))
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
        var access = await LoadAuthorizedAsync(examId, userId, role, ct);
        if (access.AccessDenied)
            return Result<SubmitExamResponse>.Failure(ErrorCode.Forbidden, "exams.forbidden");
        if (access.Exam is null)
            return Result<SubmitExamResponse>.Failure(ErrorCode.NotFound, "exams.not_found");

        var exam = access.Exam;
        if (exam.Status != ExamStatuses.InProgress)
            return Result<SubmitExamResponse>.Failure(ErrorCode.Conflict, "exams.already_submitted");

        if (request?.Answers is { Count: > 0 })
            MergeAnswers(exam, request.Answers);

        ExamTimer.ApplyElapsed(exam, request?.ElapsedSeconds);

        exam.SubmittedAt = DateTime.UtcNow;
        exam.ElapsedSeconds = Math.Min(exam.ElapsedSeconds, ExamTimer.TotalSeconds(exam));
        ExamGrading.ApplyAutoGrading(exam);

        if (!await _exams.ReplaceAsync(exam, ct))
            return Result<SubmitExamResponse>.Failure(ErrorCode.NotFound, "exams.not_found");

        return Result<SubmitExamResponse>.Success(new SubmitExamResponse(
            exam.Id,
            exam.Status,
            exam.SubmittedAt.Value,
            exam.AutoGradedScore,
            exam.IsFullyGraded,
            exam.TotalScore));
    }

    public async Task<Result<GradeExamResponse>> GradeAsync(
        string examId, string userId, string role, GradeExamRequest request, CancellationToken ct = default)
    {
        var access = await LoadAuthorizedAsync(examId, userId, role, ct);
        if (access.AccessDenied)
            return Result<GradeExamResponse>.Failure(ErrorCode.Forbidden, "exams.forbidden");
        if (access.Exam is null)
            return Result<GradeExamResponse>.Failure(ErrorCode.NotFound, "exams.not_found");

        var exam = access.Exam;
        if (exam.Status == ExamStatuses.InProgress)
            return Result<GradeExamResponse>.Failure(ErrorCode.Conflict, "exams.still_in_progress");
        if (exam.IsFullyGraded && exam.Status == ExamStatuses.Graded)
            return Result<GradeExamResponse>.Failure(ErrorCode.Conflict, "exams.already_graded");

        if (request.EssayScores is null || request.EssayScores.Count == 0)
            return Result<GradeExamResponse>.Failure(ErrorCode.Validation, "exams.essay_scores_required");

        foreach (var input in request.EssayScores)
        {
            var question = exam.QuestionsSnapshot.FirstOrDefault(q => q.Id == input.QuestionId);
            if (question is null || question.Type != QuestionTypes.Essay)
                return Result<GradeExamResponse>.Failure(ErrorCode.Validation, "exams.invalid_essay_question");
            if (input.EarnedPoints < 0 || input.EarnedPoints > question.Points)
                return Result<GradeExamResponse>.Failure(ErrorCode.Validation, "exams.invalid_essay_score");
        }

        var pairs = request.EssayScores.Select(s => (s.QuestionId, s.EarnedPoints)).ToList();
        ExamGrading.ApplyEssayScores(exam, pairs, request.Finalize);

        if (request.Finalize)
        {
            var essayIds = exam.QuestionsSnapshot.Where(q => q.Type == QuestionTypes.Essay).Select(q => q.Id).ToHashSet();
            var allGraded = essayIds.All(id => exam.Scores.Any(s => s.QuestionId == id && !s.IsAutoGraded));
            if (!allGraded)
                return Result<GradeExamResponse>.Failure(ErrorCode.Validation, "exams.essays_not_all_graded");
        }

        if (!await _exams.ReplaceAsync(exam, ct))
            return Result<GradeExamResponse>.Failure(ErrorCode.NotFound, "exams.not_found");

        return Result<GradeExamResponse>.Success(new GradeExamResponse(
            exam.Id, exam.Status, exam.TotalScore, exam.IsFullyGraded));
    }

    private sealed record ExamAccessResult(Exam? Exam, bool AccessDenied);

    private async Task<ExamAccessResult> LoadAuthorizedAsync(string examId, string userId, string role, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(examId))
            return new ExamAccessResult(null, false);

        var exam = await _exams.GetByIdAsync(examId, ct);
        if (exam is null)
            return new ExamAccessResult(null, false);

        if (!UserIdentity.CanAccessExam(userId, role, exam.ConductedBy))
            return new ExamAccessResult(null, true);

        return new ExamAccessResult(exam, false);
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

    private static ExamListItemDto MapListItem(Exam exam) => new(
        exam.Id,
        exam.CandidateName,
        exam.CandidateEmail,
        exam.PositionName,
        exam.PositionId,
        exam.Status,
        exam.StartedAt,
        exam.SubmittedAt,
        exam.TotalScore,
        exam.MaxScore,
        exam.AutoGradedScore,
        exam.IsFullyGraded,
        exam.ConductedByName);

    internal static ExamSessionResponse MapSession(Exam exam)
    {
        var questions = exam.QuestionsSnapshot
            .Select((q, i) => new ExamSessionQuestionDto(
                q.Id,
                q.Type,
                q.Text,
                q.Points,
                q.Choices?.Select(c => new ExamSessionChoiceDto(c.Id, c.Text)).ToList(),
                i,
                q.PartitionId,
                q.PartitionName))
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
            exam.ElapsedSeconds,
            ExamTimer.RemainingSeconds(exam),
            exam.StartedAt,
            exam.Status,
            questions,
            answers);
    }

    internal static ExamDetailResponse MapDetail(Exam exam)
    {
        var answerLookup = exam.Answers.ToDictionary(a => a.QuestionId);
        var questions = exam.QuestionsSnapshot
            .Select((q, i) =>
            {
                answerLookup.TryGetValue(q.Id, out var answer);
                var isCorrect = ExamGrading.IsAutoAnswerCorrect(q, answer);
                return new ExamAnswerReviewDto(
                    q.Id,
                    q.Text,
                    q.Type,
                    q.Points,
                    i,
                    q.Choices?.Select(c => new ExamSessionChoiceDto(c.Id, c.Text)).ToList(),
                    ExamGrading.FormatCandidateAnswer(q, answer),
                    ExamGrading.FormatCorrectAnswer(q),
                    isCorrect,
                    ExamGrading.GetEarnedPoints(exam, q.Id),
                    q.Type != QuestionTypes.Essay,
                    q.PartitionId,
                    q.PartitionName);
            })
            .OrderBy(q => q.Order)
            .ToList();

        return new ExamDetailResponse(
            exam.Id,
            exam.CandidateName,
            exam.CandidateEmail,
            exam.CandidateMobile,
            exam.PositionName,
            exam.Status,
            exam.StartedAt,
            exam.SubmittedAt,
            exam.TotalScore,
            exam.MaxScore,
            exam.AutoGradedScore,
            exam.IsFullyGraded,
            exam.ConductedByName,
            questions);
    }
}
