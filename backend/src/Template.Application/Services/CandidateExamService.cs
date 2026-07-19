using HireExam.Application.Common;
using HireExam.Contracts.Exams;
using HireExam.Contracts.Invitations;
using HireExam.Core.Entities;
using HireExam.Core.Interfaces;

namespace HireExam.Application.Services;

public interface ICandidateExamService
{
    Task<Result<InviteInfoResponse>> GetInviteInfoAsync(string token, CancellationToken ct = default);
    Task<Result<ExamSessionResponse>> StartAsync(string token, StartCandidateExamRequest request, CancellationToken ct = default);
    Task<Result<ExamSessionResponse>> GetSessionAsync(string token, CancellationToken ct = default);
    Task<Result<ExamSessionResponse>> SaveAnswersAsync(string token, SaveAnswersRequest request, CancellationToken ct = default);
    Task<Result<SubmitExamResponse>> SubmitAsync(string token, SubmitExamRequest? request, CancellationToken ct = default);
}

public sealed class CandidateExamService : ICandidateExamService
{
    private readonly IExamInvitationRepository _invitations;
    private readonly IExamRepository _exams;
    private readonly IPositionRepository _positions;
    private readonly IExamTemplateRepository _templates;

    public CandidateExamService(
        IExamInvitationRepository invitations,
        IExamRepository exams,
        IPositionRepository positions,
        IExamTemplateRepository templates)
    {
        _invitations = invitations;
        _exams = exams;
        _positions = positions;
        _templates = templates;
    }

    public async Task<Result<InviteInfoResponse>> GetInviteInfoAsync(string token, CancellationToken ct = default)
    {
        var invitation = await LoadInvitationAsync(token, ct);
        if (invitation is null)
            return Result<InviteInfoResponse>.Failure(ErrorCode.NotFound, "invitations.not_found");

        var now = DateTime.UtcNow;
        var displayStatus = InvitationService.ResolveDisplayStatus(invitation, now);
        var isCompleted = displayStatus == InvitationStatuses.Completed;
        var isExpired = displayStatus == InvitationStatuses.Expired;
        var canResume = displayStatus == InvitationStatuses.InProgress;
        var canStart = displayStatus == InvitationStatuses.Pending && !isExpired;

        return Result<InviteInfoResponse>.Success(new InviteInfoResponse(
            invitation.PositionName,
            displayStatus,
            canStart,
            canResume,
            isCompleted,
            isExpired,
            invitation.LinkExpiresAt));
    }

    public async Task<Result<ExamSessionResponse>> StartAsync(
        string token, StartCandidateExamRequest request, CancellationToken ct = default)
    {
        var invitation = await LoadInvitationAsync(token, ct);
        if (invitation is null)
            return Result<ExamSessionResponse>.Failure(ErrorCode.NotFound, "invitations.not_found");

        var now = DateTime.UtcNow;
        if (InvitationService.ResolveDisplayStatus(invitation, now) == InvitationStatuses.Completed)
            return Result<ExamSessionResponse>.Failure(ErrorCode.Conflict, "invitations.already_used");
        if (invitation.Status == InvitationStatuses.InProgress && !string.IsNullOrEmpty(invitation.ExamId))
            return Result<ExamSessionResponse>.Failure(ErrorCode.Conflict, "invitations.already_started");
        if (invitation.LinkExpiresAt <= now)
            return Result<ExamSessionResponse>.Failure(ErrorCode.Conflict, "invitations.expired");

        var fullName = request.FullName?.Trim() ?? string.Empty;
        var email = request.Email?.Trim() ?? string.Empty;
        var mobile = request.Mobile?.Trim() ?? string.Empty;
        if (fullName.Length < 2)
            return Result<ExamSessionResponse>.Failure(ErrorCode.Validation, "invitations.full_name_required");
        if (email.Length < 5 || !email.Contains('@'))
            return Result<ExamSessionResponse>.Failure(ErrorCode.Validation, "invitations.email_invalid");
        if (mobile.Length < 6)
            return Result<ExamSessionResponse>.Failure(ErrorCode.Validation, "invitations.mobile_required");

        var (exam, errorCode) = await ExamFactory.CreateFromTemplateAsync(
            _positions,
            _templates,
            invitation.PositionId,
            invitation.CreatedBy,
            invitation.CreatedByName,
            fullName,
            email,
            mobile,
            invitation.Id,
            ct);
        if (exam is null)
            return Result<ExamSessionResponse>.Failure(ErrorCode.Validation, errorCode!);

        await _exams.InsertAsync(exam, ct);

        invitation.Status = InvitationStatuses.InProgress;
        invitation.ExamId = exam.Id;
        await _invitations.ReplaceAsync(invitation, ct);

        return Result<ExamSessionResponse>.Success(ExamService.MapSession(exam));
    }

    public async Task<Result<ExamSessionResponse>> GetSessionAsync(string token, CancellationToken ct = default)
    {
        var loaded = await LoadInProgressExamAsync(token, ct);
        if (loaded.Error is not null)
            return Result<ExamSessionResponse>.Failure(loaded.Error.Value.Code, loaded.Error.Value.Message);

        var exam = loaded.Exam!;
        if (ExamTimer.IsExpired(exam))
        {
            await FinalizeSubmitAsync(exam, loaded.Invitation!, null, ct);
            return Result<ExamSessionResponse>.Failure(ErrorCode.Conflict, "exams.time_expired");
        }

        return Result<ExamSessionResponse>.Success(ExamService.MapSession(exam));
    }

    public async Task<Result<ExamSessionResponse>> SaveAnswersAsync(
        string token, SaveAnswersRequest request, CancellationToken ct = default)
    {
        var loaded = await LoadInProgressExamAsync(token, ct);
        if (loaded.Error is not null)
            return Result<ExamSessionResponse>.Failure(loaded.Error.Value.Code, loaded.Error.Value.Message);

        var exam = loaded.Exam!;
        ExamTimer.ApplyElapsed(exam, request.ElapsedSeconds);
        if (ExamTimer.IsExpired(exam))
        {
            if (request.Answers is { Count: > 0 })
                MergeAnswers(exam, request.Answers);
            await FinalizeSubmitAsync(exam, loaded.Invitation!, request.Answers, ct);
            return Result<ExamSessionResponse>.Failure(ErrorCode.Conflict, "exams.time_expired");
        }

        if (request.Answers is null || request.Answers.Count == 0)
            return Result<ExamSessionResponse>.Failure(ErrorCode.Validation, "exams.answers_required");

        MergeAnswers(exam, request.Answers);
        if (!await _exams.ReplaceAsync(exam, ct))
            return Result<ExamSessionResponse>.Failure(ErrorCode.NotFound, "exams.not_found");

        return Result<ExamSessionResponse>.Success(ExamService.MapSession(exam));
    }

    public async Task<Result<SubmitExamResponse>> SubmitAsync(
        string token, SubmitExamRequest? request, CancellationToken ct = default)
    {
        var loaded = await LoadInProgressExamAsync(token, ct);
        if (loaded.Error is not null)
            return Result<SubmitExamResponse>.Failure(loaded.Error.Value.Code, loaded.Error.Value.Message);

        var exam = loaded.Exam!;
        ExamTimer.ApplyElapsed(exam, request?.ElapsedSeconds);
        if (request?.Answers is { Count: > 0 })
            MergeAnswers(exam, request.Answers);

        await FinalizeSubmitAsync(exam, loaded.Invitation!, null, ct);
        return Result<SubmitExamResponse>.Success(new SubmitExamResponse(
            exam.Id,
            exam.Status,
            exam.SubmittedAt!.Value,
            exam.AutoGradedScore,
            exam.IsFullyGraded,
            exam.TotalScore));
    }

    private async Task<ExamInvitation?> LoadInvitationAsync(string token, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(token)) return null;
        return await _invitations.GetByTokenAsync(token.Trim(), ct);
    }

    private sealed record LoadedExam(Exam? Exam, ExamInvitation? Invitation, (ErrorCode Code, string Message)? Error);

    private async Task<LoadedExam> LoadInProgressExamAsync(string token, CancellationToken ct)
    {
        var invitation = await LoadInvitationAsync(token, ct);
        if (invitation is null)
            return new LoadedExam(null, null, (ErrorCode.NotFound, "invitations.not_found"));

        if (InvitationService.ResolveDisplayStatus(invitation, DateTime.UtcNow) == InvitationStatuses.Completed)
            return new LoadedExam(null, invitation, (ErrorCode.Conflict, "invitations.already_used"));

        if (string.IsNullOrEmpty(invitation.ExamId))
            return new LoadedExam(null, invitation, (ErrorCode.Conflict, "invitations.not_started"));

        var exam = await _exams.GetByIdAsync(invitation.ExamId, ct);
        if (exam is null)
            return new LoadedExam(null, invitation, (ErrorCode.NotFound, "exams.not_found"));

        if (exam.Status != ExamStatuses.InProgress)
            return new LoadedExam(null, invitation, (ErrorCode.Conflict, "exams.already_submitted"));

        return new LoadedExam(exam, invitation, null);
    }

    private async Task FinalizeSubmitAsync(
        Exam exam,
        ExamInvitation invitation,
        IReadOnlyList<ExamAnswerInputDto>? answers,
        CancellationToken ct)
    {
        if (answers is { Count: > 0 })
            MergeAnswers(exam, answers);

        exam.SubmittedAt = DateTime.UtcNow;
        exam.ElapsedSeconds = Math.Min(exam.ElapsedSeconds, ExamTimer.TotalSeconds(exam));
        ExamGrading.ApplyAutoGrading(exam);
        await _exams.ReplaceAsync(exam, ct);

        invitation.Status = InvitationStatuses.Completed;
        await _invitations.ReplaceAsync(invitation, ct);
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
}
