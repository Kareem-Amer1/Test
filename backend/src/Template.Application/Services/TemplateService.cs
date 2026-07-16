using HireExam.Application.Common;
using HireExam.Contracts.Templates;
using HireExam.Core.Entities;
using HireExam.Core.Interfaces;

namespace HireExam.Application.Services;

public interface ITemplateService
{
    Task<Result<TemplateResponse>> GetByPositionIdAsync(string positionId, CancellationToken ct = default);
    Task<Result<TemplateResponse>> UpdateDurationAsync(string positionId, string userId, UpdateDurationRequest request, CancellationToken ct = default);
    Task<Result<TemplateResponse>> AddQuestionAsync(string positionId, string userId, UpsertQuestionRequest request, CancellationToken ct = default);
    Task<Result<TemplateResponse>> UpdateQuestionAsync(string positionId, string userId, string questionId, UpsertQuestionRequest request, CancellationToken ct = default);
    Task<Result<TemplateResponse>> DeleteQuestionAsync(string positionId, string userId, string questionId, CancellationToken ct = default);
    Task<Result<TemplateResponse>> ReorderQuestionsAsync(string positionId, string userId, ReorderQuestionsRequest request, CancellationToken ct = default);
}

public sealed class TemplateService : ITemplateService
{
    private readonly IPositionRepository _positions;
    private readonly IExamTemplateRepository _templates;

    public TemplateService(IPositionRepository positions, IExamTemplateRepository templates)
    {
        _positions = positions;
        _templates = templates;
    }

    public async Task<Result<TemplateResponse>> GetByPositionIdAsync(string positionId, CancellationToken ct = default)
    {
        var template = await LoadTemplateAsync(positionId, ct);
        if (template is null)
            return Result<TemplateResponse>.Failure(ErrorCode.NotFound, "templates.not_found");
        return Result<TemplateResponse>.Success(Map(template));
    }

    public async Task<Result<TemplateResponse>> UpdateDurationAsync(string positionId, string userId, UpdateDurationRequest request, CancellationToken ct = default)
    {
        if (request.DurationMinutes < 1 || request.DurationMinutes > 480)
            return Result<TemplateResponse>.Failure(ErrorCode.Validation, "templates.duration_invalid");

        var template = await LoadTemplateAsync(positionId, ct);
        if (template is null)
            return Result<TemplateResponse>.Failure(ErrorCode.NotFound, "templates.not_found");

        template.DurationMinutes = request.DurationMinutes;
        Touch(template, userId);
        await _templates.ReplaceAsync(template, ct);
        return Result<TemplateResponse>.Success(Map(template));
    }

    public async Task<Result<TemplateResponse>> AddQuestionAsync(string positionId, string userId, UpsertQuestionRequest request, CancellationToken ct = default)
    {
        var template = await LoadTemplateAsync(positionId, ct);
        if (template is null)
            return Result<TemplateResponse>.Failure(ErrorCode.NotFound, "templates.not_found");

        var build = BuildQuestion(request, template.Questions.Count);
        if (!build.IsSuccess)
            return Result<TemplateResponse>.Failure(build.Error.Code, build.Error.Message);

        template.Questions.Add(build.Value!);
        Touch(template, userId);
        await _templates.ReplaceAsync(template, ct);
        return Result<TemplateResponse>.Success(Map(template));
    }

    public async Task<Result<TemplateResponse>> UpdateQuestionAsync(string positionId, string userId, string questionId, UpsertQuestionRequest request, CancellationToken ct = default)
    {
        var template = await LoadTemplateAsync(positionId, ct);
        if (template is null)
            return Result<TemplateResponse>.Failure(ErrorCode.NotFound, "templates.not_found");

        var idx = template.Questions.FindIndex(q => q.Id == questionId);
        if (idx < 0)
            return Result<TemplateResponse>.Failure(ErrorCode.NotFound, "templates.question_not_found");

        var build = BuildQuestion(request, template.Questions[idx].Order, questionId);
        if (!build.IsSuccess)
            return Result<TemplateResponse>.Failure(build.Error.Code, build.Error.Message);

        template.Questions[idx] = build.Value!;
        Touch(template, userId);
        await _templates.ReplaceAsync(template, ct);
        return Result<TemplateResponse>.Success(Map(template));
    }

    public async Task<Result<TemplateResponse>> DeleteQuestionAsync(string positionId, string userId, string questionId, CancellationToken ct = default)
    {
        var template = await LoadTemplateAsync(positionId, ct);
        if (template is null)
            return Result<TemplateResponse>.Failure(ErrorCode.NotFound, "templates.not_found");

        var removed = template.Questions.RemoveAll(q => q.Id == questionId);
        if (removed == 0)
            return Result<TemplateResponse>.Failure(ErrorCode.NotFound, "templates.question_not_found");

        ReindexOrders(template.Questions);
        Touch(template, userId);
        await _templates.ReplaceAsync(template, ct);
        return Result<TemplateResponse>.Success(Map(template));
    }

    public async Task<Result<TemplateResponse>> ReorderQuestionsAsync(string positionId, string userId, ReorderQuestionsRequest request, CancellationToken ct = default)
    {
        var template = await LoadTemplateAsync(positionId, ct);
        if (template is null)
            return Result<TemplateResponse>.Failure(ErrorCode.NotFound, "templates.not_found");

        if (request.QuestionIds.Count != template.Questions.Count)
            return Result<TemplateResponse>.Failure(ErrorCode.Validation, "templates.reorder_invalid");

        var lookup = template.Questions.ToDictionary(q => q.Id);
        if (request.QuestionIds.Any(id => !lookup.ContainsKey(id)))
            return Result<TemplateResponse>.Failure(ErrorCode.Validation, "templates.reorder_invalid");

        var reordered = request.QuestionIds.Select((id, i) =>
        {
            var q = lookup[id];
            q.Order = i;
            return q;
        }).ToList();

        template.Questions = reordered;
        Touch(template, userId);
        await _templates.ReplaceAsync(template, ct);
        return Result<TemplateResponse>.Success(Map(template));
    }

    private async Task<ExamTemplate?> LoadTemplateAsync(string positionId, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(positionId))
            return null;
        var position = await _positions.GetByIdAsync(positionId, ct);
        if (position is null)
            return null;
        return await _templates.GetByPositionIdAsync(positionId, ct);
    }

    private static void Touch(ExamTemplate template, string userId)
    {
        template.LastModifiedAt = DateTime.UtcNow;
        template.LastModifiedBy = userId;
    }

    private static void ReindexOrders(List<TemplateQuestion> questions)
    {
        for (var i = 0; i < questions.Count; i++)
            questions[i].Order = i;
    }

    private static Result<TemplateQuestion> BuildQuestion(UpsertQuestionRequest request, int order, string? existingId = null)
    {
        var type = request.Type?.Trim() ?? string.Empty;
        var text = request.Text?.Trim() ?? string.Empty;
        if (text.Length < 3)
            return Result<TemplateQuestion>.Failure(ErrorCode.Validation, "templates.question_text_required");
        if (request.Points < 1)
            return Result<TemplateQuestion>.Failure(ErrorCode.Validation, "templates.points_invalid");
        if (type is not (QuestionTypes.Essay or QuestionTypes.TrueFalse or QuestionTypes.Mcq))
            return Result<TemplateQuestion>.Failure(ErrorCode.Validation, "templates.question_type_invalid");

        var question = new TemplateQuestion
        {
            Id = existingId ?? Guid.NewGuid().ToString("N"),
            Type = type,
            Text = text,
            Points = request.Points,
            Order = order,
        };

        if (type == QuestionTypes.TrueFalse)
        {
            if (request.CorrectAnswer is null)
                return Result<TemplateQuestion>.Failure(ErrorCode.Validation, "templates.correct_answer_required");
            question.CorrectAnswer = request.CorrectAnswer;
        }
        else if (type == QuestionTypes.Mcq)
        {
            var choices = (request.Choices ?? Array.Empty<McqChoiceInput>())
                .Where(c => !string.IsNullOrWhiteSpace(c.Text))
                .Select(c => new McqChoice
                {
                    Id = string.IsNullOrWhiteSpace(c.Id) ? Guid.NewGuid().ToString("N") : c.Id!,
                    Text = c.Text.Trim(),
                })
                .ToList();
            if (choices.Count < 2)
                return Result<TemplateQuestion>.Failure(ErrorCode.Validation, "templates.choices_required");
            if (string.IsNullOrWhiteSpace(request.CorrectChoiceId) ||
                choices.All(c => c.Id != request.CorrectChoiceId))
                return Result<TemplateQuestion>.Failure(ErrorCode.Validation, "templates.correct_choice_required");
            question.Choices = choices;
            question.CorrectChoiceId = request.CorrectChoiceId;
        }

        return Result<TemplateQuestion>.Success(question);
    }

    internal static TemplateResponse Map(ExamTemplate template) =>
        new(
            template.Id,
            template.PositionId,
            template.DurationMinutes,
            template.Questions
                .OrderBy(q => q.Order)
                .Select(q => new TemplateQuestionDto(
                    q.Id,
                    q.Type,
                    q.Text,
                    q.Points,
                    q.CorrectAnswer,
                    q.Choices?.Select(c => new McqChoiceDto(c.Id, c.Text)).ToList(),
                    q.CorrectChoiceId,
                    q.Order))
                .ToList(),
            template.LastModifiedAt);
}
