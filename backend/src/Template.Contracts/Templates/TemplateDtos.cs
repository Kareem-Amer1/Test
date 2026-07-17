namespace HireExam.Contracts.Templates;

public sealed record McqChoiceDto(string Id, string Text);

public sealed record TemplateQuestionDto(
    string Id,
    string Type,
    string Text,
    int Points,
    bool? CorrectAnswer,
    IReadOnlyList<McqChoiceDto>? Choices,
    string? CorrectChoiceId,
    int Order);

public sealed record TemplatePartitionDto(
    string Id,
    string Name,
    int Order,
    IReadOnlyList<TemplateQuestionDto> Questions);

public sealed record TemplateResponse(
    string Id,
    string PositionId,
    int DurationMinutes,
    IReadOnlyList<TemplatePartitionDto> Partitions,
    DateTime LastModifiedAt);

public sealed record UpdateDurationRequest(int DurationMinutes);

public sealed record UpsertPartitionRequest(string Name);

public sealed record McqChoiceInput(string? Id, string Text);

public sealed record UpsertQuestionRequest(
    string Type,
    string Text,
    int Points,
    bool? CorrectAnswer,
    IReadOnlyList<McqChoiceInput>? Choices,
    string? CorrectChoiceId);

public sealed record ReorderQuestionsRequest(IReadOnlyList<string> QuestionIds);
